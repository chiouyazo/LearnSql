using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Typing;

public sealed class ExpressionTypeInferrer
{
    private readonly FragmentTree _tree;
    private readonly VariableScope _variables;
    private readonly RelationShapeResolver _relations;

    public ExpressionTypeInferrer(FragmentTree tree, ISchemaCatalog schema, VariableScope variables)
    {
        _tree = tree;
        _variables = variables;
        _relations = new RelationShapeResolver(tree, schema, this);
    }

    public ValueKind Infer(ScalarExpression? expression)
    {
        switch (expression)
        {
            case null:
                return ValueKind.Unknown;
            case NullLiteral:
                return ValueKind.Null;
            case StringLiteral:
                return ValueKind.Text;
            case IntegerLiteral:
            case NumericLiteral:
            case RealLiteral:
            case MoneyLiteral:
                return ValueKind.Numeric;
            case BinaryLiteral:
                return ValueKind.Binary;
            case ParenthesisExpression parenthesis:
                return Infer(parenthesis.Expression);
            case UnaryExpression:
                return ValueKind.Numeric;
            case BinaryExpression binary:
                return InferBinary(binary);
            case ColumnReferenceExpression column:
                return _relations.ResolveColumnKind(column, _tree.GetNode(column));
            case VariableReference variable:
                return _variables.TryGet(variable.Name, out DeclaredVariable? declared)
                    ? declared!.Kind
                    : ValueKind.Unknown;
            case FunctionCall call:
                return InferFunction(call);
            case LeftFunctionCall:
            case RightFunctionCall:
                return ValueKind.Text;
            case CastCall cast:
                return TSqlDataTypes.GetKind(cast.DataType);
            case ConvertCall convert:
                return TSqlDataTypes.GetKind(convert.DataType);
            case SearchedCaseExpression searched:
                return Combine(
                    searched
                        .WhenClauses.Select(clause => clause.ThenExpression)
                        .Append(searched.ElseExpression)
                );
            case SimpleCaseExpression simple:
                return Combine(
                    simple
                        .WhenClauses.Select(clause => clause.ThenExpression)
                        .Append(simple.ElseExpression)
                );
            case IIfCall conditional:
                return Combine([conditional.ThenExpression, conditional.ElseExpression]);
            case CoalesceExpression coalesce:
                return Combine(coalesce.Expressions);
            case NullIfExpression nullIf:
                return Infer(nullIf.FirstExpression);
            case ScalarSubquery subquery:
                return InferSubquery(subquery);
            case ParameterlessCall parameterless:
                return
                    parameterless.ParameterlessCallType
                        is ParameterlessCallType.CurrentTimestamp
                            or ParameterlessCallType.CurrentDate
                    ? ValueKind.DateTime
                    : ValueKind.Text;
            default:
                return ValueKind.Unknown;
        }
    }

    private ValueKind InferBinary(BinaryExpression binary)
    {
        switch (binary.BinaryExpressionType)
        {
            case BinaryExpressionType.Concat:
                return ValueKind.Text;
            case BinaryExpressionType.Add:
                return InferAddition(Infer(binary.FirstExpression), Infer(binary.SecondExpression));
            case BinaryExpressionType.Subtract:
                ValueKind left = Infer(binary.FirstExpression);
                ValueKind right = Infer(binary.SecondExpression);
                return left == ValueKind.DateTime && right == ValueKind.Numeric
                    ? ValueKind.DateTime
                    : ValueKind.Numeric;
            default:
                return ValueKind.Numeric;
        }
    }

    private static ValueKind InferAddition(ValueKind left, ValueKind right)
    {
        switch (AdditionClassifier.Classify(left, right))
        {
            case AdditionMeaning.Concatenation:
                return ValueKind.Text;
            case AdditionMeaning.DateArithmetic:
                return ValueKind.DateTime;
            case AdditionMeaning.Numeric:
                return ValueKind.Numeric;
            default:
                return ValueKind.Unknown;
        }
    }

    private ValueKind InferFunction(FunctionCall call)
    {
        string name = call.FunctionName.Value;
        if (FunctionResultKinds.TryGetFixedKind(name, out ValueKind fixedKind))
            return fixedKind;

        if (FunctionResultKinds.IsFirstArgumentTyped(name))
            return call.Parameters.Count > 0 ? Infer(call.Parameters[0]) : ValueKind.Unknown;

        if (FunctionResultKinds.IsThirdArgumentTyped(name))
            return call.Parameters.Count > 2 ? Infer(call.Parameters[2]) : ValueKind.Unknown;

        if (FunctionResultKinds.IsNullReplacement(name))
            return InferNullReplacement(call);

        return ValueKind.Unknown;
    }

    private ValueKind InferNullReplacement(FunctionCall call)
    {
        if (call.Parameters.Count < 2)
            return ValueKind.Unknown;

        ValueKind first = Infer(call.Parameters[0]);
        return first == ValueKind.Null ? Infer(call.Parameters[1]) : first;
    }

    private ValueKind InferSubquery(ScalarSubquery subquery)
    {
        RelationShape? shape = _relations.ShapeOfQuery(subquery.QueryExpression);
        return shape is { Columns.Count: > 0 } ? shape.Columns[0].Kind : ValueKind.Unknown;
    }

    private ValueKind Combine(IEnumerable<ScalarExpression?> expressions)
    {
        ValueKind result = ValueKind.Unknown;
        bool sawKind = false;
        foreach (ScalarExpression? expression in expressions)
        {
            ValueKind kind = Infer(expression);
            if (expression is null || kind == ValueKind.Null)
                continue;
            if (kind == ValueKind.Unknown)
                return ValueKind.Unknown;
            if (!sawKind)
            {
                result = kind;
                sawKind = true;
            }
            else if (result != kind)
            {
                return ValueKind.Unknown;
            }
        }
        return result;
    }
}

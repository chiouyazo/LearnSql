using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class SetVariableRule : TranslationRule<SetVariableStatement>
{
    protected override bool AppliesTo(
        SetVariableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return statement.Expression is not null
            && statement.CursorDefinition is null
            && statement.Variable is not null
            && statement.AssignmentKind != AssignmentKind.BitwiseXorEquals;
    }

    protected override void Translate(
        SetVariableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.UsesVariables = true;
        string name = statement.Variable.Name;
        if (!context.Variables.TryGet(name, out DeclaredVariable? variable))
            return;

        context.Replace(
            statement,
            () => VariableStorage.Assign(name, BuildValue(statement, variable!, context))
        );
    }

    private static string BuildValue(
        SetVariableStatement statement,
        DeclaredVariable variable,
        TranslationContext context
    )
    {
        string expression = context.Render(statement.Expression);
        if (statement.AssignmentKind == AssignmentKind.Equals)
            return SqliteCastBuilder.Build(
                expression,
                variable.DataType,
                null,
                SqliteCastBuilder.VariableTextLength
            );

        string current = "(" + VariableStorage.Read(variable.Name) + ")";
        string operand = "(" + expression + ")";
        string combined = statement.AssignmentKind switch
        {
            AssignmentKind.DivideEquals => ArithmeticSqlBuilder.Divide(current, operand),
            AssignmentKind.ModEquals => ArithmeticSqlBuilder.Modulo(current, operand),
            _ => current + " " + OperatorFor(statement, variable, context) + " " + operand,
        };
        return SqliteCastBuilder.Build(
            combined,
            variable.DataType,
            null,
            SqliteCastBuilder.VariableTextLength
        );
    }

    private static string OperatorFor(
        SetVariableStatement statement,
        DeclaredVariable variable,
        TranslationContext context
    )
    {
        switch (statement.AssignmentKind)
        {
            case AssignmentKind.AddEquals:
                ValueKind right = context.Types.Infer(statement.Expression);
                return
                    AdditionClassifier.Classify(variable.Kind, right)
                    == AdditionMeaning.Concatenation
                    ? "||"
                    : "+";
            case AssignmentKind.SubtractEquals:
                return "-";
            case AssignmentKind.MultiplyEquals:
                return "*";
            case AssignmentKind.BitwiseAndEquals:
                return "&";
            case AssignmentKind.BitwiseOrEquals:
                return "|";
            default:
                return "||";
        }
    }
}

using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DeclareVariableRule : TranslationRule<DeclareVariableStatement>
{
    private const int AlreadyDeclaredErrorNumber = 134;

    protected override void Translate(
        DeclareVariableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.UsesVariables = true;

        foreach (DeclareVariableElement element in statement.Declarations)
        {
            string name = element.VariableName.Value;
            if (context.Variables.TryGet(name, out DeclaredVariable? _))
            {
                context.Report(
                    element,
                    DiagnosticKind.TranslationError,
                    DiagnosticSeverity.Error,
                    "VariableAlreadyDeclared",
                    $"The variable name '{name}' has already been declared. Variable names must be unique within a query batch or stored procedure.",
                    null,
                    AlreadyDeclaredErrorNumber
                );
                continue;
            }
            context.Variables.Declare(name, element.DataType, statement);
        }

        context.Replace(statement, () => BuildInsert(statement, context));
    }

    private static string BuildInsert(
        DeclareVariableStatement statement,
        TranslationContext context
    )
    {
        List<KeyValuePair<string, string>> rows = [];
        foreach (DeclareVariableElement element in statement.Declarations)
        {
            string initial = element.Value is null
                ? "NULL"
                : SqliteCastBuilder.Build(
                    context.Render(element.Value),
                    element.DataType,
                    null,
                    SqliteCastBuilder.VariableTextLength
                );
            rows.Add(new KeyValuePair<string, string>(element.VariableName.Value, initial));
        }
        return VariableStorage.Declare(rows);
    }
}

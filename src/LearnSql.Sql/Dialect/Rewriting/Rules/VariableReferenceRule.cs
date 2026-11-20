using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class VariableReferenceRule : TranslationRule<VariableReference>
{
    private const int UndeclaredErrorNumber = 137;

    protected override void Translate(
        VariableReference reference,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.UsesVariables = true;

        bool declared = context.Variables.TryGet(reference.Name, out DeclaredVariable? variable);
        if (!declared || IsInsideDeclaringStatement(variable!, node))
        {
            context.Report(
                reference,
                DiagnosticKind.TranslationError,
                DiagnosticSeverity.Error,
                "UndeclaredVariable",
                $"Must declare the scalar variable \"{reference.Name}\".",
                null,
                UndeclaredErrorNumber
            );
            return;
        }

        context.Replace(reference, VariableStorage.Read(reference.Name));
    }

    private static bool IsInsideDeclaringStatement(DeclaredVariable variable, FragmentNode node)
    {
        return node.Ancestors().Any(ancestor => ancestor.Fragment == variable.DeclaredBy);
    }
}

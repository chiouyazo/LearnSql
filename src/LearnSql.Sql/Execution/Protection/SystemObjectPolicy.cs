using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Resources;

namespace LearnSql.Sql.Execution;

public sealed class SystemObjectPolicy : IStatementPolicy
{
    private const string InternalPrefix = "sqlite_";

    private static readonly string[] InformationSchemaViews = ["COLUMNS", "TABLES"];

    public PolicyViolation? Evaluate(TranslatedStatement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);

        if (statement.IsGenerated)
        {
            return null;
        }

        foreach (string name in statement.EffectiveProfile.ModifiedObjects)
        {
            string? message = Describe(name);
            if (message is not null)
            {
                return new PolicyViolation(statement, name, message);
            }
        }

        return null;
    }

    private static string? Describe(string name)
    {
        if (name.StartsWith(InternalPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ExecutionStrings.ProtectedInternalObject(name);
        }

        if (InformationSchemaViews.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return ExecutionStrings.ProtectedInformationSchemaView(name);
        }

        if (string.Equals(name, VariableStorage.TableName, StringComparison.OrdinalIgnoreCase))
        {
            return ExecutionStrings.ProtectedVariableTable(name);
        }

        return null;
    }
}

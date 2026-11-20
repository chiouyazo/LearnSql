using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.MsSql;

public static class MsSqlTransactionErrors
{
    public const string NoActiveTransactionMarker = "no transaction is active";

    public static MsSqlError? TryCreate(
        StatementKind kind,
        string sqliteMessage,
        int? lineNumber = null
    )
    {
        ArgumentNullException.ThrowIfNull(sqliteMessage);

        if (!sqliteMessage.Contains(NoActiveTransactionMarker, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return kind switch
        {
            StatementKind.CommitTransaction => new MsSqlError(
                3902,
                16,
                1,
                "The COMMIT TRANSACTION request has no corresponding BEGIN TRANSACTION.",
                lineNumber
            ),
            StatementKind.RollbackTransaction => new MsSqlError(
                3903,
                16,
                1,
                "The ROLLBACK TRANSACTION request has no corresponding BEGIN TRANSACTION.",
                lineNumber
            ),
            _ => null,
        };
    }
}

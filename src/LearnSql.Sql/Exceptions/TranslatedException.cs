using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Exceptions;

public class TranslatedException : Exception
{
    public TranslatedException(
        SqliteError sqliteError,
        MsSqlError error,
        Exception? innerException = null
    )
        : base(GetMessage(error), innerException)
    {
        ArgumentNullException.ThrowIfNull(sqliteError);
        ArgumentNullException.ThrowIfNull(error);

        SqliteError = sqliteError;
        Error = error;
    }

    public SqliteError SqliteError { get; }

    public MsSqlError Error { get; }

    private static string GetMessage(MsSqlError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.ToString();
    }
}

using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Translation;

public interface IErrorTranslator
{
    Language Language { get; }

    MsSqlError Translate(SqliteError error, int? lineNumber = null, ErrorContext? context = null);

    TranslatedException CreateException(
        SqliteException exception,
        int? lineNumber = null,
        ErrorContext? context = null
    );
}

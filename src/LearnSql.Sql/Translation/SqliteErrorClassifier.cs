using System.Collections.Frozen;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Translation;

public static class SqliteErrorClassifier
{
    public const int ConstraintResultCode = 19;

    public const int ConstraintCheckExtendedCode = 275;
    public const int ConstraintForeignKeyExtendedCode = 787;
    public const int ConstraintNotNullExtendedCode = 1299;
    public const int ConstraintPrimaryKeyExtendedCode = 1555;
    public const int ConstraintUniqueExtendedCode = 2067;

    private static readonly IReadOnlyDictionary<string, string> NoArguments = FrozenDictionary<
        string,
        string
    >.Empty;

    public static SqliteError Classify(SqliteException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return Classify(
            exception.SqliteErrorCode,
            exception.SqliteExtendedErrorCode,
            exception.Message
        );
    }

    public static SqliteError Classify(int errorCode, int extendedErrorCode, string message)
    {
        SqliteError mapped = SqliteErrorMapper.Match(message);
        if (errorCode != ConstraintResultCode)
        {
            return mapped;
        }

        SqliteErrorType? expected = extendedErrorCode switch
        {
            ConstraintCheckExtendedCode => SqliteErrorType.CheckConstraintFailed,
            ConstraintForeignKeyExtendedCode => SqliteErrorType.ForeignKeyConstraintFailed,
            ConstraintNotNullExtendedCode => SqliteErrorType.NotNullConstraintFailed,
            ConstraintPrimaryKeyExtendedCode => SqliteErrorType.PrimaryKeyConstraintFailed,
            ConstraintUniqueExtendedCode => SqliteErrorType.UniqueConstraintFailed,
            _ => null,
        };

        if (expected is not SqliteErrorType expectedType || mapped.Type == expectedType)
        {
            return mapped;
        }

        if (
            expectedType == SqliteErrorType.PrimaryKeyConstraintFailed
            && mapped.Type == SqliteErrorType.UniqueConstraintFailed
        )
        {
            return mapped with { Type = expectedType };
        }

        return new SqliteError(expectedType, mapped.Message, NoArguments);
    }
}

using LearnSql.Sql.Generated;

namespace LearnSql.Sql.Models;

public sealed record SqliteError(
    SqliteErrorType Type,
    string Message,
    IReadOnlyDictionary<string, string> Arguments
);

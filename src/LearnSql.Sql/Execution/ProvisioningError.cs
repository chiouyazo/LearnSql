using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Execution;

public sealed record ProvisioningError(
    ProvisioningErrorKind Kind,
    string Message,
    string? Resource = null,
    string? Detail = null,
    MsSqlError? MsSql = null,
    SqliteError? Sqlite = null
);

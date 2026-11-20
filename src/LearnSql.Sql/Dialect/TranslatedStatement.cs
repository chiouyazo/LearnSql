using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Dialect;

public sealed record TranslatedStatement(
    string Sql,
    SourceSpan Origin,
    bool IsGenerated,
    StatementProfile? Profile = null
)
{
    public StatementProfile EffectiveProfile => Profile ?? StatementProfile.Unknown;
}

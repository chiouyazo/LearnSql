namespace LearnSql.SqlFormat;

internal sealed record SqlStatement(IReadOnlyList<SqlToken> Tokens, bool IsBatchSeparator);

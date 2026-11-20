namespace LearnSql.Sql.Dialect.Diagnostics;

public readonly record struct SourceSpan(SourcePosition Start, SourcePosition End)
{
    public int Length => End.Offset - Start.Offset;
}

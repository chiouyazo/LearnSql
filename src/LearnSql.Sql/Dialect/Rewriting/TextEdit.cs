namespace LearnSql.Sql.Dialect.Rewriting;

public sealed record TextEdit(
    int Start,
    int Length,
    EditPlacement Placement,
    int Sequence,
    Func<string> Text
)
{
    public int End => Start + Length;
}

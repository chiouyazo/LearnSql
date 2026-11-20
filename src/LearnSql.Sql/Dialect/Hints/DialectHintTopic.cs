using LearnSql.Sql.Resources;

namespace LearnSql.Sql.Dialect.Hints;

public sealed class DialectHintTopic
{
    private readonly Func<IReadOnlyList<string>> _points;

    internal DialectHintTopic(
        string id,
        IReadOnlyList<DialectHintExample> examples,
        Func<IReadOnlyList<string>>? points = null
    )
    {
        Id = id;
        Examples = examples;
        _points = points ?? (() => HintStrings.Points(id));
    }

    public string Id { get; }

    public string Title => HintStrings.Title(Id);

    public string Summary => HintStrings.Summary(Id);

    public IReadOnlyList<string> Points => _points();

    public IReadOnlyList<DialectHintExample> Examples { get; }
}

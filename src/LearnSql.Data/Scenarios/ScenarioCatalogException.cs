namespace LearnSql.Data.Scenarios;

public sealed class ScenarioCatalogException : Exception
{
    public ScenarioCatalogException(
        ScenarioCatalogErrorKind kind,
        string message,
        string? scenarioId = null,
        string? resource = null,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        Kind = kind;
        ScenarioId = scenarioId;
        Resource = resource;
    }

    public ScenarioCatalogErrorKind Kind { get; }
    public string? ScenarioId { get; }
    public string? Resource { get; }
}

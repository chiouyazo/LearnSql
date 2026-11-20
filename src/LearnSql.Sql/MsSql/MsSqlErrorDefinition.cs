namespace LearnSql.Sql.MsSql;

public sealed record MsSqlErrorDefinition(
    int Number,
    int Severity,
    int State,
    string Template,
    string? ContextTemplate = null
)
{
    public MsSqlError Create(string message, int? lineNumber = null) =>
        new MsSqlError(Number, Severity, State, message, lineNumber);
}

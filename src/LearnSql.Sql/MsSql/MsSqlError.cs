namespace LearnSql.Sql.MsSql;

public sealed record MsSqlError(
    int Number,
    int Severity,
    int State,
    string Message,
    int? LineNumber = null
)
{
    public string Header =>
        LineNumber is int line
            ? $"Msg {Number}, Level {Severity}, State {State}, Line {line}"
            : $"Msg {Number}, Level {Severity}, State {State}";

    public override string ToString() => $"{Header}\n{Message}";
}

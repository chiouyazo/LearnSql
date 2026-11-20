using LearnSql.Sql.Dialect.Diagnostics;

namespace LearnSql.Sql.Execution;

public sealed record ExecutionResult(
    IReadOnlyList<StatementResult> Statements,
    IReadOnlyList<DialectDiagnostic> Diagnostics,
    ExecutionError? Error,
    TimeSpan Duration,
    bool RolledBack
)
{
    public bool Succeeded => Error is null;

    public bool Truncated => Statements.Any((StatementResult statement) => statement.Truncated);

    public IReadOnlyList<ResultSet> ResultSets =>
        [.. Statements.SelectMany((StatementResult statement) => statement.ResultSets)];

    public IReadOnlyList<string> Messages
    {
        get
        {
            List<string> messages =
            [
                .. Statements.SelectMany((StatementResult statement) => statement.Messages),
            ];
            if (messages.Count == 0 && Error is null && Statements.Count > 0)
            {
                messages.Add(ExecutionMessages.CommandsCompleted);
            }

            return messages;
        }
    }
}

namespace LearnSql.Sql.Dialect.Hints;

public sealed class DialectHintExample
{
    private readonly Func<string> _msSqlResult;
    private readonly Func<string> _sandboxResult;

    public DialectHintExample(string statement, string msSqlResult, string sandboxResult)
        : this(statement, () => msSqlResult, () => sandboxResult) { }

    internal DialectHintExample(
        string statement,
        Func<string> msSqlResult,
        Func<string> sandboxResult
    )
    {
        Statement = statement;
        _msSqlResult = msSqlResult;
        _sandboxResult = sandboxResult;
    }

    public string Statement { get; }

    public string MsSqlResult => _msSqlResult();

    public string SandboxResult => _sandboxResult();
}

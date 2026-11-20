using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Support;

public sealed class RecordingProgress : IProgress<SandboxProgress>
{
    private readonly List<SandboxProgress> _reports = [];

    public IReadOnlyList<SandboxProgress> Reports => _reports;

    public void Report(SandboxProgress value)
    {
        _reports.Add(value);
    }
}

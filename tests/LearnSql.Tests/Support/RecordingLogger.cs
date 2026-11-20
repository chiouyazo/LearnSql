using Microsoft.Extensions.Logging;

namespace LearnSql.Tests.Support;

public sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

public sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _gate = new object();

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return [.. _entries];
            }
        }
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        lock (_gate)
        {
            _entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }
}

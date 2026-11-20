using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace LearnSql.Sql.Execution;

internal sealed class ExecutionGuard : IDisposable
{
    private const int InterruptRequested = 1;
    private const int ContinueExecution = 0;

    private readonly SandboxConnection _connection;
    private readonly TimeSpan _timeout;
    private readonly CancellationToken _cancellationToken;
    private readonly TimeProvider _timeProvider;
    private readonly SandboxAuthorizationPolicy _policy;
    private readonly ILogger _logger;
    private readonly long _start;
    private readonly delegate_progress _progressCallback;
    private readonly delegate_authorizer _authorizerCallback;
    private bool _progressInstalled;
    private bool _authorizerInstalled;

    public ExecutionGuard(
        SandboxConnection connection,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        TimeProvider timeProvider,
        SandboxAuthorizationPolicy policy,
        ILogger logger
    )
    {
        _connection = connection;
        _timeout = timeout;
        _cancellationToken = cancellationToken;
        _timeProvider = timeProvider;
        _policy = policy;
        _logger = logger;
        _start = timeProvider.GetTimestamp();
        _progressCallback = OnProgress;
        _authorizerCallback = OnAuthorize;
    }

    public GuardAbortReason Reason { get; private set; }

    public TimeSpan Timeout => _timeout;

    public void Install(int progressInstructions)
    {
        try
        {
            raw.sqlite3_progress_handler(
                _connection.Handle,
                progressInstructions,
                _progressCallback,
                null
            );
            _progressInstalled = true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "The SQLite progress handler is unavailable. Long running statements cannot be interrupted."
            );
        }

        try
        {
            int result = raw.sqlite3_set_authorizer(_connection.Handle, _authorizerCallback, null);
            _authorizerInstalled = result == raw.SQLITE_OK;
            if (!_authorizerInstalled)
            {
                _logger.LogWarning(
                    "The SQLite authorizer could not be installed (result code {ResultCode}).",
                    result
                );
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "The SQLite authorizer is unavailable. Engine level restrictions are not active."
            );
        }
    }

    public void Poll()
    {
        _cancellationToken.ThrowIfCancellationRequested();
        if (IsExpired())
        {
            Reason = GuardAbortReason.TimedOut;
            throw new ExecutionTimeoutException(_timeout);
        }
    }

    public void Dispose()
    {
        if (_progressInstalled)
        {
            raw.sqlite3_progress_handler(_connection.Handle, 0, null, null);
            _progressInstalled = false;
        }

        if (_authorizerInstalled)
        {
            raw.sqlite3_set_authorizer(_connection.Handle, (delegate_authorizer?)null, null);
            _authorizerInstalled = false;
        }
    }

    private bool IsExpired()
    {
        return _timeout != System.Threading.Timeout.InfiniteTimeSpan
            && _timeProvider.GetElapsedTime(_start) >= _timeout;
    }

    private int OnProgress(object? userData)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            Reason = GuardAbortReason.Cancelled;
            return InterruptRequested;
        }

        if (IsExpired())
        {
            Reason = GuardAbortReason.TimedOut;
            return InterruptRequested;
        }

        return ContinueExecution;
    }

    private int OnAuthorize(
        object? userData,
        int actionCode,
        utf8z firstArgument,
        utf8z secondArgument,
        utf8z databaseName,
        utf8z innermostViewOrTrigger
    )
    {
        return (int)
            _policy.Decide(
                actionCode,
                firstArgument.utf8_to_string(),
                secondArgument.utf8_to_string()
            );
    }
}

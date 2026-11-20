namespace LearnSql.Sql.Execution;

public sealed class TimeSliceYield : ICooperativeYield
{
    private static readonly TimeSpan YieldDelay = TimeSpan.FromMilliseconds(1);

    private readonly TimeSpan _interval;
    private readonly TimeProvider _timeProvider;
    private long _sliceStart;

    public TimeSliceYield(TimeSpan interval, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(interval, TimeSpan.Zero);

        _interval = interval;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _sliceStart = _timeProvider.GetTimestamp();
    }

    public void StartSlice()
    {
        _sliceStart = _timeProvider.GetTimestamp();
    }

    public ValueTask YieldIfDueAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_timeProvider.GetElapsedTime(_sliceStart) < _interval)
        {
            return ValueTask.CompletedTask;
        }

        return YieldAsync(cancellationToken);
    }

    public async ValueTask YieldAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(YieldDelay, _timeProvider, cancellationToken);
        _sliceStart = _timeProvider.GetTimestamp();
    }
}

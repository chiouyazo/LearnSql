using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Results;

public partial class ElapsedTime : IDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(100);

    private CancellationTokenSource? _ticker;

    [Parameter]
    public bool Running { get; set; }

    [Parameter]
    public TimeSpan Duration { get; set; }

    [Inject]
    private IQueryRunner Runner { get; set; } = default!;

    private TimeSpan CurrentDuration => Running ? Runner.Elapsed : Duration;

    protected override void OnParametersSet()
    {
        if (Running && _ticker is null)
        {
            _ticker = new CancellationTokenSource();
            _ = TickAsync(_ticker.Token);
        }
        else if (!Running)
        {
            StopTicker();
        }
    }

    public void Dispose()
    {
        StopTicker();
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(Interval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private void StopTicker()
    {
        _ticker?.Cancel();
        _ticker?.Dispose();
        _ticker = null;
    }

    private static string Format(TimeSpan duration)
    {
        return duration.TotalSeconds < 1
            ? $"{duration.TotalMilliseconds:0} ms"
            : $"{duration.TotalSeconds:0.0} s";
    }
}

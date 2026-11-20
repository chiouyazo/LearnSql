using LearnSql.Core.Services.Drafts;
using LearnSql.Core.Services.Execution;
using LearnSql.Core.Services.Guided;
using LearnSql.Core.Services.Templates;
using LearnSql.Core.Services.Workspace;
using LearnSql.Core.State;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Core;

public sealed class GuidedCoordinatorTests
{
    private sealed class ManualTimer : ITimer
    {
        private readonly TimerCallback _callback;
        private readonly object? _state;

        public ManualTimer(TimerCallback callback, object? state)
        {
            _callback = callback;
            _state = state;
        }

        public bool Disposed { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return true;
        }

        public void Fire()
        {
            _callback(_state);
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ManualTime : TimeProvider
    {
        public List<ManualTimer> Timers { get; } = [];

        public int PendingCount => Timers.Count((ManualTimer timer) => !timer.Disposed);

        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period
        )
        {
            ManualTimer timer = new ManualTimer(callback, state);
            Timers.Add(timer);
            return timer;
        }

        public void FireAll()
        {
            foreach (ManualTimer timer in Timers.Where((ManualTimer t) => !t.Disposed).ToArray())
            {
                timer.Fire();
            }
        }
    }

    private sealed class FakeDrafts : IDraftStore
    {
        public Dictionary<string, string> Texts { get; } = new Dictionary<string, string>();

        public Task<string?> LoadAsync(
            string scenarioId,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(Texts.GetValueOrDefault(scenarioId));
        }

        public void Save(string scenarioId, string text)
        {
            Texts[scenarioId] = text;
        }

        public Task FlushAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEditor : IWorkspaceEditor
    {
        private readonly FakeDrafts _drafts;
        private readonly string _scenarioId;

        public FakeEditor(FakeDrafts drafts, string scenarioId)
        {
            _drafts = drafts;
            _scenarioId = scenarioId;
        }

        public bool IsAvailable => true;

        public Task InsertAtCursorAsync(string text)
        {
            return Task.CompletedTask;
        }

        public Task ReplaceTextAsync(string text)
        {
            _drafts.Save(_scenarioId, text);
            return Task.CompletedTask;
        }

        public Task FocusAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTemplates : IQueryTemplateFactory
    {
        public string CreateStarterQuery(DatabaseSchema schema)
        {
            return "SELECT starter";
        }

        public IReadOnlyList<QueryTemplate> Create(DatabaseSchema schema)
        {
            return [];
        }
    }

    private sealed class FakeSession : IAppSession
    {
        public FakeSession(ScenarioDefinition scenario)
        {
            Scenario = scenario;
        }

        public AppPhase Phase { get; set; } = AppPhase.Ready;

        public AppOperation? Operation => null;

        public SandboxProgress? Progress => null;

        public AppFailure? Failure => null;

        public ScenarioDefinition Scenario { get; set; }

        public IReadOnlyList<ScenarioDefinition> Scenarios => [Scenario];

        public bool IsReady => Phase == AppPhase.Ready;

        public event EventHandler? StateChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<DatabaseProvisionedEventArgs>? DatabaseProvisioned;

        public event EventHandler? SchemaChanged
        {
            add { }
            remove { }
        }

        public Task RestoreScenarioAsync()
        {
            return Task.CompletedTask;
        }

        public void SelectInitialScenario(string scenarioId) { }

        public Task EnsureStartedAsync()
        {
            return Task.CompletedTask;
        }

        public Task RetryAsync()
        {
            return Task.CompletedTask;
        }

        public Task SwitchScenarioAsync(string scenarioId)
        {
            return Task.CompletedTask;
        }

        public Task ResetDatabaseAsync()
        {
            DatabaseProvisioned?.Invoke(
                this,
                new DatabaseProvisionedEventArgs(Scenario, AppOperation.ResetDatabase)
            );
            return Task.CompletedTask;
        }

        public void NotifySchemaChanged() { }

        public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DatabaseSchema.Empty);
        }

        public void RaiseProvisioned(ScenarioDefinition scenario)
        {
            Scenario = scenario;
            DatabaseProvisioned?.Invoke(
                this,
                new DatabaseProvisionedEventArgs(scenario, AppOperation.SwitchScenario)
            );
        }
    }

    private sealed class FakeRunner : IQueryRunner
    {
        public bool IsRunning { get; private set; }

        public TimeSpan Elapsed => TimeSpan.Zero;

        public QueryRun? LastRun { get; private set; }

        public ExecutionOptions Options { get; set; } = ExecutionOptions.Default;

        public List<string> Executed { get; } = [];

        public TaskCompletionSource? Gate { get; set; }

        public int RowsPerRun { get; set; } = 1;

        public event EventHandler? Changed;

        public event EventHandler<QueryRun>? Completed;

        public async Task<QueryRunStatus> RunAsync(
            string sql,
            SourceOrigin? origin = null,
            CancellationToken cancellationToken = default
        )
        {
            if (IsRunning)
            {
                return QueryRunStatus.AlreadyRunning;
            }

            IsRunning = true;
            Changed?.Invoke(this, EventArgs.Empty);
            if (Gate is not null)
            {
                await Gate.Task;
            }

            Executed.Add(sql);
            IsRunning = false;
            object?[][] rows =
            [
                .. Enumerable.Range(0, RowsPerRun).Select((int i) => new object?[] { i }),
            ];
            StatementResult statement = new StatementResult(
                0,
                0,
                StatementKind.Select,
                default,
                [new SqlResultSet([], rows, false)],
                null,
                TimeSpan.Zero,
                []
            );
            QueryRun run = new QueryRun(
                sql,
                SourceOrigin.Start,
                new ExecutionResult([statement], [], null, TimeSpan.Zero, false),
                Options
            );
            LastRun = run;
            Changed?.Invoke(this, EventArgs.Empty);
            Completed?.Invoke(this, run);
            return QueryRunStatus.Completed;
        }

        public void Cancel() { }

        public void ClearLastRun() { }
    }

    private sealed class Rig
    {
        public Rig(int steps = 3, int expectedRows = 1)
        {
            Scenario = ScenarioRoutesTests.Scenario("starter", 1);
            Drafts = new FakeDrafts();
            Session = new FakeSession(Scenario);
            Runner = new FakeRunner();
            Time = new ManualTime();
            Storage = new ExerciseMemoryStorage();
            Coordinator = new GuidedCoordinator(
                Session,
                Runner,
                new FakeEditor(Drafts, "starter"),
                Drafts,
                Storage,
                new FakeTemplates(),
                Time
            );
            Definition = new GuidedScenarioDefinition
            {
                Id = "g1",
                Title = "Titel",
                Description = "Beschreibung",
                Topic = ExerciseTopic.Select,
                RequiresFreshDatabase = false,
                Steps =
                [
                    .. Enumerable
                        .Range(1, steps)
                        .Select(
                            (int i) =>
                                new GuidedStep
                                {
                                    Id = "g1-" + i,
                                    Title = "Schritt " + i,
                                    Sql = "SELECT\n    " + i + ";",
                                    Kind = GuidedStepKind.ReadOnly,
                                    QueryExplanation = new QueryExplanation
                                    {
                                        Summary = "s",
                                        Parts = [],
                                    },
                                    ResultExplanation = "r",
                                    DatabaseEffect = "e",
                                    ExpectedResultRows = expectedRows,
                                }
                        ),
                ],
            };
        }

        public ScenarioDefinition Scenario { get; }

        public FakeDrafts Drafts { get; }

        public FakeSession Session { get; }

        public FakeRunner Runner { get; }

        public ManualTime Time { get; }

        public ExerciseMemoryStorage Storage { get; }

        public GuidedCoordinator Coordinator { get; }

        public GuidedScenarioDefinition Definition { get; }

        public async Task<GuidedCoordinator> StartedAsync()
        {
            Assert.Equal(GuidedActionResult.Done, await Coordinator.StartAsync(Definition));
            return Coordinator;
        }

        public async Task FireCountdownAsync()
        {
            Time.FireAll();
            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task Start_InsertsMultilineSqlRunsItAndPlays()
    {
        Rig rig = new Rig();

        GuidedCoordinator coordinator = await rig.StartedAsync();

        Assert.Equal(0, coordinator.Machine!.CurrentIndex);
        Assert.True(coordinator.Machine.CurrentCompleted);
        Assert.True(coordinator.IsPlaying);
        Assert.True(coordinator.IsCountingDown);
        Assert.Equal("SELECT\n    1;", rig.Drafts.Texts["starter"]);
        Assert.Equal(["SELECT\n    1;"], rig.Runner.Executed);
    }

    [Fact]
    public async Task Pause_StopsTheCountdownAndIsIdempotent()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();

        coordinator.Pause();
        coordinator.Pause();

        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsCountingDown);
        Assert.Equal(0, rig.Time.PendingCount);
        await rig.FireCountdownAsync();
        Assert.Equal(0, coordinator.Machine!.CurrentIndex);
    }

    [Fact]
    public async Task Countdown_WhilePlaying_AdvancesAndRunsTheNextStep()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();

        await rig.FireCountdownAsync();

        Assert.Equal(1, coordinator.Machine!.CurrentIndex);
        Assert.Equal(2, rig.Runner.Executed.Count);
        Assert.True(coordinator.IsCountingDown);
    }

    [Fact]
    public async Task Resume_AfterPause_AdvancesOnceEvenWhenCalledTwice()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();
        coordinator.Pause();

        Task<GuidedActionResult> first = coordinator.ResumeAsync();
        Task<GuidedActionResult> second = coordinator.ResumeAsync();
        await Task.WhenAll(first, second);

        Assert.Equal(GuidedActionResult.Done, first.Result);
        Assert.Equal(GuidedActionResult.Done, second.Result);
        Assert.Equal(1, coordinator.Machine!.CurrentIndex);
        Assert.Equal(2, rig.Runner.Executed.Count);
        Assert.True(coordinator.IsPlaying);
    }

    [Fact]
    public async Task PauseAndResume_MashedManyTimes_EndsInAConsistentState()
    {
        Rig rig = new Rig(steps: 6);
        GuidedCoordinator coordinator = await rig.StartedAsync();

        for (int index = 0; index < 4; index++)
        {
            coordinator.Pause();
            await coordinator.ResumeAsync();
            coordinator.Pause();
        }

        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsBusy);
        Assert.Equal(0, rig.Time.PendingCount);
        Assert.Equal(coordinator.Machine!.CurrentIndex + 1, rig.Runner.Executed.Count);
    }

    [Fact]
    public async Task Pause_WhileAStepRuns_StopsAfterThatStepWithoutCountdown()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();
        coordinator.Pause();
        rig.Runner.Gate = new TaskCompletionSource();
        Task<GuidedActionResult> next = coordinator.NextAsync();

        coordinator.Pause();
        rig.Runner.Gate.SetResult();
        await next;

        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsCountingDown);
        Assert.False(coordinator.IsBusy);
        Assert.Equal(1, coordinator.Machine!.CurrentIndex);
    }

    [Fact]
    public async Task Resume_WhileAStepRuns_IsRejectedWithoutBlocking()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();
        coordinator.Pause();
        rig.Runner.Gate = new TaskCompletionSource();
        Task<GuidedActionResult> next = coordinator.NextAsync();

        GuidedActionResult resume = await coordinator.ResumeAsync();
        rig.Runner.Gate.SetResult();
        await next;

        Assert.Equal(GuidedActionResult.Busy, resume);
        Assert.False(coordinator.IsBusy);
    }

    [Fact]
    public async Task Previous_PausesAndRerunsReadOnlySteps()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();
        await coordinator.NextAsync();

        GuidedActionResult result = await coordinator.PreviousAsync();

        Assert.Equal(GuidedActionResult.Done, result);
        Assert.Equal(0, coordinator.Machine!.CurrentIndex);
        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsCountingDown);
        Assert.Equal(3, rig.Runner.Executed.Count);
        Assert.Equal(GuidedActionResult.Unavailable, await coordinator.PreviousAsync());
    }

    [Fact]
    public async Task Next_OnTheLastStep_IsUnavailableAndPlaybackStops()
    {
        Rig rig = new Rig(steps: 2);
        GuidedCoordinator coordinator = await rig.StartedAsync();

        await rig.FireCountdownAsync();

        Assert.True(coordinator.Machine!.IsLast);
        Assert.True(coordinator.Machine.IsFinished);
        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsCountingDown);
        Assert.Equal(GuidedActionResult.Unavailable, await coordinator.NextAsync());
        Assert.Equal(GuidedActionResult.Done, await coordinator.ResumeAsync());
        Assert.False(coordinator.IsPlaying);
    }

    [Fact]
    public async Task Deviation_StopsPlayingButAllowsManualNext()
    {
        Rig rig = new Rig(expectedRows: 5);
        GuidedCoordinator coordinator = await rig.StartedAsync();

        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsCountingDown);
        Assert.Equal(GuidedCheckKind.RowCountDiffers, coordinator.Machine!.CurrentCheck!.Kind);
        Assert.Equal(GuidedActionResult.Done, await coordinator.NextAsync());
    }

    [Fact]
    public async Task End_ClearsTheRunAndLaterCallsAreHarmless()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();

        await coordinator.EndAsync();
        await coordinator.EndAsync();
        coordinator.Pause();

        Assert.Null(coordinator.Machine);
        Assert.False(coordinator.IsPlaying);
        Assert.Equal(0, rig.Time.PendingCount);
        Assert.Equal(GuidedActionResult.Unavailable, await coordinator.ResumeAsync());
        Assert.Equal(GuidedActionResult.Unavailable, await coordinator.NextAsync());
        Assert.Equal(GuidedActionResult.Unavailable, await coordinator.PreviousAsync());
    }

    [Fact]
    public async Task ScenarioSwitch_EndsTheRunAndCancelsTheCountdown()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();

        rig.Session.RaiseProvisioned(ScenarioRoutesTests.Scenario("analytics", 2));
        await Task.Delay(50);
        coordinator.Pause();

        Assert.Null(coordinator.Machine);
        Assert.False(coordinator.IsPlaying);
        Assert.Equal(0, rig.Time.PendingCount);
        Assert.Equal(GuidedNotice.ScenarioSwitched, coordinator.Notice);
    }

    [Fact]
    public async Task UserText_IsBackedUpAndAnOccupiedBackupBlocksWithoutADialog()
    {
        Rig rig = new Rig();
        GuidedCoordinator coordinator = await rig.StartedAsync();
        coordinator.Pause();

        rig.Drafts.Save("starter", "SELECT 'mine'");
        GuidedActionResult first = await coordinator.NextAsync();
        Assert.Equal(GuidedActionResult.Done, first);
        Assert.True(coordinator.HasBackup);

        rig.Drafts.Save("starter", "SELECT 'newer'");
        GuidedActionResult second = await coordinator.NextAsync();

        Assert.Equal(GuidedActionResult.BackupOccupied, second);
        Assert.Equal(GuidedNotice.BackupOccupied, coordinator.Notice);
        Assert.Equal(1, coordinator.Machine!.CurrentIndex);
        Assert.False(coordinator.IsPlaying);
        Assert.False(coordinator.IsBusy);
        Assert.Equal("SELECT 'newer'", rig.Drafts.Texts["starter"]);

        await coordinator.DiscardBackupAsync();
        Assert.Equal(GuidedActionResult.Done, await coordinator.NextAsync());
    }

    [Fact]
    public async Task Start_WithStarterQueryInTheEditor_DoesNotCreateABackup()
    {
        Rig rig = new Rig();
        rig.Drafts.Save("starter", "SELECT starter");

        await rig.StartedAsync();

        Assert.False(rig.Coordinator.HasBackup);
    }
}

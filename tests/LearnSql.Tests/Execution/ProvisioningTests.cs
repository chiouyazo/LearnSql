using LearnSql.Data.Scenarios;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using Microsoft.Extensions.Logging;

namespace LearnSql.Tests.Execution;

public class ProvisioningTests
{
    private static readonly string[] KnownScenarios = ["starter", "analytics", "playground"];

    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    private static ScenarioCatalog FakeCatalog(
        Action<FakeResourceAssembly>? customize = null,
        params Dictionary<string, object?>[] scenarios
    )
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(scenarios);
        customize?.Invoke(assembly);
        return ScenarioCatalog.Load(assembly);
    }

    private static Dictionary<string, object?> Good(string id = "good")
    {
        return ManifestJson.Scenario(
            id,
            1,
            [$"Data/{id}/Schema/01.sql"],
            [$"Data/{id}/Views/01.sql"],
            [$"Data/{id}/Seed/01.sql"]
        );
    }

    private static void AddGoodScripts(FakeResourceAssembly assembly, string id = "good")
    {
        assembly.Add(
            ScenarioCatalog.ToResourceName($"Data/{id}/Schema/01.sql"),
            "CREATE TABLE Item (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);"
        );
        assembly.Add(
            ScenarioCatalog.ToResourceName($"Data/{id}/Views/01.sql"),
            "CREATE VIEW ItemNames AS SELECT Name FROM Item;"
        );
        assembly.Add(
            ScenarioCatalog.ToResourceName($"Data/{id}/Seed/01.sql"),
            "INSERT INTO Item VALUES (1, 'one'), (2, 'two');"
        );
    }

    [Fact]
    public void KnownScenarios_AreTheThreeShippedOnes()
    {
        Assert.Equal(
            KnownScenarios.Order(StringComparer.Ordinal),
            ScenarioData
                .Catalog.Scenarios.Select((ScenarioDefinition s) => s.Id)
                .Order(StringComparer.Ordinal)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public async Task ProvisionScenarioAsync_Scenario_SucceedsAndFillsTheDatabase(string scenarioId)
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        ProvisionResult result = await database.ProvisionScenarioAsync(scenarioId, progress);

        Assert.True(result.Succeeded, result.Error?.Message);
        Assert.Equal(scenarioId, result.ScenarioId);
        Assert.Equal(scenarioId, database.Status.ScenarioId);
        Assert.True(database.Status.IsInitialized);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.NotEmpty(schema.Tables);
        foreach (RelationSchema table in schema.Tables.Where((RelationSchema t) => !t.IsSystem))
        {
            Assert.True(await database.CountRowsAsync(table.Name) > 0, table.Name + " is empty.");
        }
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public async Task ProvisionScenarioAsync_Progress_EndsWithReady(string scenarioId)
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync(scenarioId, progress);

        SandboxProgress last = progress.Reports[^1];
        Assert.Equal(SandboxPhase.Ready, last.Phase);
        Assert.Equal(last.TotalSteps, last.Step);
        Assert.Equal(1.0, last.Fraction);
        Assert.Equal(SandboxPhase.ResettingDatabase, progress.Reports[0].Phase);
        Assert.All(
            progress.Reports,
            (SandboxProgress p) => Assert.False(string.IsNullOrWhiteSpace(p.Description))
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public async Task ProvisionScenarioAsync_OnAnInitializedSandbox_ReportsMonotonePhasesAndSteps(
        string scenarioId
    )
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.InitializeAsync();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync(scenarioId, progress);

        Assert.NotEmpty(progress.Reports);
        for (int index = 1; index < progress.Reports.Count; index++)
        {
            SandboxProgress previous = progress.Reports[index - 1];
            SandboxProgress current = progress.Reports[index];
            Assert.True(current.Phase >= previous.Phase, $"{previous.Phase} then {current.Phase}");
            Assert.True(current.Step >= previous.Step, $"{previous.Step} then {current.Step}");
            Assert.Equal(previous.TotalSteps, current.TotalSteps);
        }

        Assert.Equal(SandboxPhase.Ready, progress.Reports[^1].Phase);
        Assert.Contains(
            progress.Reports,
            (SandboxProgress p) => p.Phase == SandboxPhase.CreatingSchema
        );
        Assert.Contains(
            progress.Reports,
            (SandboxProgress p) => p.Phase == SandboxPhase.SeedingData
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public async Task ProvisionScenarioAsync_OnAFreshSandbox_ReportsMonotoneProgressAndExactlyOneReady(
        string scenarioId
    )
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync(scenarioId, progress);

        Assert.Equal(
            1,
            progress.Reports.Count((SandboxProgress p) => p.Phase == SandboxPhase.Ready)
        );
        Assert.Equal(SandboxPhase.Ready, progress.Reports[^1].Phase);
        Assert.Equal(0.0, progress.Reports[0].Fraction);
        for (int index = 1; index < progress.Reports.Count; index++)
        {
            SandboxProgress previous = progress.Reports[index - 1];
            SandboxProgress current = progress.Reports[index];
            Assert.True(current.Fraction >= previous.Fraction, $"{previous} then {current}");
            Assert.True(current.Phase >= previous.Phase, $"{previous.Phase} then {current.Phase}");
        }
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public async Task ProvisionScenarioAsync_Progress_FractionsStayWithinZeroAndOne(
        string scenarioId
    )
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync(scenarioId, progress);

        Assert.All(progress.Reports, (SandboxProgress p) => Assert.InRange(p.Fraction, 0.0, 1.0));
    }

    [Fact]
    public async Task ProvisionScenarioAsync_Views_AreCreatedBeforeTheSeedData()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync("starter", progress);

        List<SandboxPhase> phases = [.. progress.Reports.Select((SandboxProgress p) => p.Phase)];
        Assert.True(
            phases.IndexOf(SandboxPhase.CreatingSchema) < phases.IndexOf(SandboxPhase.SeedingData)
        );
        Assert.True(
            phases.IndexOf(SandboxPhase.ReadingSchema) < phases.LastIndexOf(SandboxPhase.Ready)
        );
    }

    [Fact]
    public async Task ProvisionScenarioAsync_SecondScenario_ReplacesTheFirst()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "playground"
        );
        await database.ExecuteOkAsync("CREATE TABLE UserTable (a INT)");
        Assert.NotNull((await database.GetSchemaAsync()).Find("Country"));

        ProvisionResult result = await database.ProvisionScenarioAsync("starter");

        Assert.True(result.Succeeded);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("Country"));
        Assert.Null(schema.Find("UserTable"));
        Assert.NotNull(schema.Find("Product"));
        Assert.Equal("starter", database.Status.ScenarioId);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_IdIsCaseInsensitive()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ProvisionResult result = await database.ProvisionScenarioAsync("STARTER");

        Assert.True(result.Succeeded, result.Error?.Message);
        Assert.Equal("starter", result.ScenarioId);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_NullId_ThrowsArgumentNullException()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            database.ProvisionScenarioAsync(null!)
        );
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("starter ")]
    public async Task ProvisionScenarioAsync_UnknownId_ReturnsUnknownScenarioAndKeepsTheState(
        string id
    )
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync(
            "CREATE TABLE UserTable (a INT); INSERT INTO UserTable VALUES (1)"
        );
        long customers = await database.CountRowsAsync("Customer");

        ProvisionResult result = await database.ProvisionScenarioAsync(id);

        Assert.False(result.Succeeded);
        Assert.Equal(ProvisioningErrorKind.UnknownScenario, result.Error?.Kind);
        Assert.Equal("starter", result.ScenarioId);
        Assert.Equal("starter", database.Status.ScenarioId);
        Assert.Equal(1L, await database.CountRowsAsync("UserTable"));
        Assert.Equal(customers, await database.CountRowsAsync("Customer"));
    }

    [Fact]
    public async Task ProvisionScenarioAsync_UnknownId_ListsTheAvailableScenarios()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ProvisionResult result = await database.ProvisionScenarioAsync("nope");

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Contains("nope", error.Message);
        Assert.All(KnownScenarios, (string id) => Assert.Contains(id, error.Message));
        Assert.Null(result.ScenarioId);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_UnknownIdOnAFreshSandbox_LeavesItEmptyButUsable()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        await database.ProvisionScenarioAsync("nope");

        Assert.Null(database.Status.ScenarioId);
        Assert.Equal(1L, await database.ScalarAsync("SELECT 1"));
    }

    [Fact]
    public async Task ProvisionScenarioAsync_ScriptFailure_ReturnsScriptFailedWithTheResource()
    {
        ScenarioCatalog catalog = FakeCatalog(
            (FakeResourceAssembly assembly) =>
            {
                AddGoodScripts(assembly);
                assembly.Add(
                    ScenarioCatalog.ToResourceName("Data/broken/Schema/01.sql"),
                    "CREATE TABLE Dup (Id INTEGER PRIMARY KEY);"
                );
                assembly.Add(
                    ScenarioCatalog.ToResourceName("Data/broken/Seed/01.sql"),
                    "INSERT INTO Dup VALUES (1); INSERT INTO Dup VALUES (1);"
                );
            },
            Good(),
            ManifestJson.Scenario(
                "broken",
                2,
                ["Data/broken/Schema/01.sql"],
                [],
                ["Data/broken/Seed/01.sql"]
            )
        );
        RecordingLogger<SandboxDatabase> logger = new RecordingLogger<SandboxDatabase>();
        await using SandboxDatabase database = SandboxFactory.Create(
            catalog: catalog,
            logger: logger
        );

        ProvisionResult result = await database.ProvisionScenarioAsync("broken");

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.ScriptFailed, error.Kind);
        Assert.Equal("Data/broken/Seed/01.sql", error.Resource);
        Assert.Equal(2627, error.MsSql?.Number);
        Assert.NotNull(error.Sqlite);
        Assert.Contains("Data/broken/Seed/01.sql", error.Message);
        Assert.Contains("UNIQUE constraint failed", error.Detail);
        Assert.Null(result.ScenarioId);
        Assert.Contains(
            logger.Entries,
            (LogEntry e) => e.Level == LogLevel.Error && e.Exception is not null
        );
    }

    [Fact]
    public async Task ProvisionScenarioAsync_ScriptFailure_KeepsThePreviousStateIncludingUserData()
    {
        ScenarioCatalog catalog = FakeCatalog(
            (FakeResourceAssembly assembly) =>
            {
                AddGoodScripts(assembly);
                assembly.Add(
                    ScenarioCatalog.ToResourceName("Data/broken/Schema/01.sql"),
                    "CREATE TABLE Half (Id INTEGER PRIMARY KEY); CREATE TABLE Half (Id INTEGER);"
                );
            },
            Good(),
            ManifestJson.Scenario("broken", 2, ["Data/broken/Schema/01.sql"])
        );
        await using SandboxDatabase database = SandboxFactory.Create(catalog: catalog);
        await database.ProvisionScenarioAsync("good");
        await database.ExecuteOkAsync("INSERT INTO Item VALUES (3, 'three')");

        ProvisionResult result = await database.ProvisionScenarioAsync("broken");

        Assert.Equal(ProvisioningErrorKind.ScriptFailed, result.Error?.Kind);
        Assert.Equal("good", result.ScenarioId);
        Assert.Equal("good", database.Status.ScenarioId);
        Assert.Equal(3L, await database.CountRowsAsync("Item"));
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("Half"));
        Assert.NotNull(schema.Find("ItemNames"));
    }

    [Fact]
    public async Task ProvisionScenarioAsync_ScriptFailureWithoutPriorScenario_LeavesAnEmptyUsableDatabase()
    {
        ScenarioCatalog catalog = FakeCatalog(
            (FakeResourceAssembly assembly) =>
                assembly.Add(
                    ScenarioCatalog.ToResourceName("Data/broken/Schema/01.sql"),
                    "CREATE TABL Oops;"
                ),
            ManifestJson.Scenario("broken", 1, ["Data/broken/Schema/01.sql"])
        );
        await using SandboxDatabase database = SandboxFactory.Create(catalog: catalog);

        ProvisionResult result = await database.ProvisionScenarioAsync("broken");

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.ScriptFailed, error.Kind);
        Assert.Equal("Data/broken/Schema/01.sql", error.Resource);
        Assert.Null(database.Status.ScenarioId);
        Assert.Empty((await database.GetSchemaAsync()).Relations);
        await database.ExecuteOkAsync("CREATE TABLE Fresh (a INT)");
    }

    [Fact]
    public async Task ProvisionScenarioAsync_ResourceVanishedAfterLoading_ReturnsInvalidScenario()
    {
        bool available = true;
        ScenarioCatalog catalog = FakeCatalog(
            (FakeResourceAssembly assembly) =>
            {
                AddGoodScripts(assembly);
                assembly.AddStream(
                    ScenarioCatalog.ToResourceName("Data/good/Seed/01.sql"),
                    () => available ? new MemoryStream("SELECT 1;"u8.ToArray()) : null
                );
            },
            Good()
        );
        await using SandboxDatabase database = SandboxFactory.Create(catalog: catalog);
        available = false;

        ProvisionResult result = await database.ProvisionScenarioAsync("good");

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.InvalidScenario, error.Kind);
        Assert.Equal("Data/good/Seed/01.sql", error.Resource);
        Assert.Contains("good", error.Message);
        Assert.Null(database.Status.ScenarioId);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_PreCancelledToken_ReturnsCancelled()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        ProvisionResult result = await database.ProvisionScenarioAsync(
            "starter",
            null,
            source.Token
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ProvisioningErrorKind.Cancelled, result.Error?.Kind);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_CancelledBetweenScripts_KeepsThePreviousState()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync("CREATE TABLE UserTable (a INT)");
        using CancellationTokenSource source = new CancellationTokenSource();
        CancelOnPhaseProgress progress = new CancelOnPhaseProgress(
            SandboxPhase.SeedingData,
            source
        );

        ProvisionResult result = await database.ProvisionScenarioAsync(
            "analytics",
            progress,
            source.Token
        );

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.Cancelled, error.Kind);
        Assert.NotNull(error.Resource);
        Assert.Equal("starter", result.ScenarioId);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.NotNull(schema.Find("UserTable"));
        Assert.NotNull(schema.Find("Customer"));
        Assert.Null(schema.Find("SalesMonthly"));
    }

    [Fact]
    public async Task ProvisionScenarioAsync_ScenarioWithoutExercisesAndGuidedScenarios_Works()
    {
        ScenarioCatalog catalog = FakeCatalog(AddGoodScriptsFor, Good());
        await using SandboxDatabase database = SandboxFactory.Create(catalog: catalog);

        ProvisionResult result = await database.ProvisionScenarioAsync("good");

        ScenarioDefinition scenario = Assert.Single(catalog.Scenarios);
        Assert.Empty(scenario.Exercises);
        Assert.Empty(scenario.GuidedScenarios);
        Assert.True(result.Succeeded, result.Error?.Message);
        Assert.Equal(2L, await database.CountRowsAsync("Item"));
        Assert.Equal(2L, await database.CountRowsAsync("ItemNames"));
    }

    private static void AddGoodScriptsFor(FakeResourceAssembly assembly)
    {
        AddGoodScripts(assembly);
    }

    [Fact]
    public async Task ResetAsync_RestoresTheScenarioAndRemovesUserDataAndVariables()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        long customers = await database.CountRowsAsync("Customer");
        await database.ExecuteOkAsync(
            "DECLARE @x INT; SET @x = 1; DELETE FROM OrderItem; DELETE FROM Orders; DELETE FROM Customer;"
        );
        await database.ExecuteOkAsync("CREATE TABLE UserTable (a INT)");
        Assert.Equal(0L, await database.CountRowsAsync("Customer"));

        ProvisionResult result = await database.ResetAsync();

        Assert.True(result.Succeeded, result.Error?.Message);
        Assert.Equal("starter", result.ScenarioId);
        Assert.Equal("starter", database.Status.ScenarioId);
        Assert.Equal(customers, await database.CountRowsAsync("Customer"));
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("UserTable"));
        ExecutionResult variables = await database.ExecuteAsync(
            "SELECT * FROM __learnsql_variables"
        );
        ExecutionError error = variables.ExpectError(ExecutionErrorKind.Sql);
        Assert.Equal(208, error.MsSql?.Number);
    }

    [Fact]
    public async Task ResetAsync_ReportsProgressEndingWithReady()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        RecordingProgress progress = new RecordingProgress();

        await database.ResetAsync(progress);

        Assert.Equal(SandboxPhase.ResettingDatabase, progress.Reports[0].Phase);
        Assert.Equal(SandboxPhase.Ready, progress.Reports[^1].Phase);
        Assert.Equal(1.0, progress.Reports[^1].Fraction);
    }

    [Fact]
    public async Task ResetAsync_WithoutAScenario_ProducesAnEmptyDatabase()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");

        ProvisionResult result = await database.ResetAsync();

        Assert.True(result.Succeeded);
        Assert.Null(result.ScenarioId);
        Assert.Empty((await database.GetSchemaAsync()).Relations);
    }

    [Fact]
    public async Task ResetAsync_OnAnUninitializedSandbox_InitializesIt()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ProvisionResult result = await database.ResetAsync();

        Assert.True(result.Succeeded);
        Assert.True(database.Status.IsInitialized);
    }

    [Fact]
    public async Task ResetAsync_TwiceInARow_YieldsTheSameData()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "analytics"
        );
        long expected = await database.CountRowsAsync("OrderLine");

        await database.ExecuteOkAsync("DELETE FROM OrderLine");
        await database.ResetAsync();
        await database.ResetAsync();

        Assert.Equal(expected, await database.CountRowsAsync("OrderLine"));
    }

    [Fact]
    public async Task InitializeAsync_ReportsProgressAndIsIdempotent()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        RecordingProgress progress = new RecordingProgress();

        await database.InitializeAsync(progress);
        int reports = progress.Reports.Count;
        await database.InitializeAsync(progress);

        Assert.Equal(SandboxPhase.PreparingEngine, progress.Reports[0].Phase);
        Assert.Equal(SandboxPhase.Ready, progress.Reports[^1].Phase);
        Assert.Equal(reports, progress.Reports.Count);
        Assert.True(database.Status.IsInitialized);
    }

    [Fact]
    public async Task StatusChanged_DuringProvisioning_ReportsTheProvisioningActivity()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        List<SandboxStatus> events = [];
        database.StatusChanged += (object? _, SandboxStatus status) => events.Add(status);

        await database.ProvisionScenarioAsync("starter");

        Assert.Contains(events, (SandboxStatus s) => s.Activity == SandboxActivity.Provisioning);
        SandboxStatus last = events[^1];
        Assert.Equal(SandboxActivity.Idle, last.Activity);
        Assert.Equal("starter", last.ScenarioId);
        Assert.True(last.IsInitialized);
    }

    private sealed class CancelOnPhaseProgress : IProgress<SandboxProgress>
    {
        private readonly SandboxPhase _phase;
        private readonly CancellationTokenSource _source;

        public CancelOnPhaseProgress(SandboxPhase phase, CancellationTokenSource source)
        {
            _phase = phase;
            _source = source;
        }

        public void Report(SandboxProgress value)
        {
            if (value.Phase == _phase)
            {
                _source.Cancel();
            }
        }
    }
}

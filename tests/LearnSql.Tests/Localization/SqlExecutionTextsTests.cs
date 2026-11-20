using LearnSql.Data.Scenarios;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Localization;

public class SqlExecutionTextsTests
{
    private const string ScenarioId = "one";

    private static ScenarioCatalog CatalogWith(string schema, string seed)
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(
            ManifestJson.Scenario(
                ScenarioId,
                1,
                ["Data/one/Schema/01.sql"],
                [],
                ["Data/one/Seed/01.sql"]
            )
        );
        assembly.Add(ScenarioCatalog.ToResourceName("Data/one/Schema/01.sql"), schema);
        assembly.Add(ScenarioCatalog.ToResourceName("Data/one/Seed/01.sql"), seed);
        return ScenarioCatalog.Load(assembly);
    }

    private static SandboxDatabase Create()
    {
        return SandboxFactory.Create(
            catalog: CatalogWith("CREATE TABLE Item (Id INTEGER PRIMARY KEY);", "SELECT 1;")
        );
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_ExplainsTheTimeLimitInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.Equal(
            "The execution was cancelled after reaching the time limit of 0.3 s. Check the query for infinite loops, for example a recursive CTE without a termination condition.",
            error.Message
        );
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_InGermanUsesTheLocalizedDecimalSeparator()
    {
        using UiCulture culture = UiCulture.Use("de");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.StartsWith(
            "Die Ausführung wurde nach 0,3 Sekunden abgebrochen (Zeitlimit).",
            error.Message
        );
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutInsideAUserTransaction_AppendsTheEnglishRollbackNote()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (a INT); BEGIN TRANSACTION; INSERT INTO T VALUES (2);"
        );

        ExecutionResult result = await database.ExecuteAsync(
            "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r) UPDATE T SET a = (SELECT COUNT(*) FROM r)",
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.EndsWith(" SQLite rolled back the open transaction.", error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledToken_ReportsTheEnglishCancellation()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1", null, source.Token);

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Cancelled);
        Assert.Equal("The execution was cancelled.", error.Message);
    }

    [Theory]
    [InlineData(
        "DROP TABLE sqlite_sequence",
        "The object \"sqlite_sequence\" is an internal system object of this environment. It cannot be modified, but you can read from it."
    )]
    [InlineData(
        "DROP VIEW COLUMNS",
        "The view \"COLUMNS\" belongs to INFORMATION_SCHEMA and is managed by this environment. It cannot be modified, dropped or recreated, but you can read from it."
    )]
    [InlineData(
        "DELETE FROM __learnsql_variables",
        "The table \"__learnsql_variables\" internally stores the values of T-SQL variables. It cannot be modified, but you can read from it."
    )]
    public async Task ExecuteAsync_ModifyingAProtectedObject_ExplainsTheProtectionInEnglish(
        string script,
        string message
    )
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync(script);

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Rejected);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ProtectedObject_InGermanStaysGerman()
    {
        using UiCulture culture = UiCulture.Use("de");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync("DROP VIEW COLUMNS");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Rejected);
        Assert.StartsWith("Die Sicht \"COLUMNS\" gehört zu INFORMATION_SCHEMA", error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DataLargerThanTheEngineAllows_ReportsTheStorageLimitInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT zeroblob(2000000000)");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.LimitExceeded);
        Assert.StartsWith("The memory limit of this environment was reached.", error.Message);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_InEnglish_ReportsTheProgressPhasesInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();
        RecordingProgress progress = new RecordingProgress();

        ProvisionResult result = await database.ProvisionScenarioAsync(ScenarioId, progress);

        Assert.True(result.Succeeded);
        Assert.Equal(
            [
                "Resetting the database",
                "Running Data/one/Schema/01.sql",
                "Running Data/one/Seed/01.sql",
                "Reading the database schema",
                "Ready",
            ],
            progress.Reports.Select((SandboxProgress p) => p.Description)
        );
    }

    [Fact]
    public async Task InitializeAsync_InEnglish_ReportsTheEnginePhasesInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();
        RecordingProgress progress = new RecordingProgress();

        await database.InitializeAsync(progress);

        Assert.Equal(
            ["Initializing SQLite", "Reading the database schema", "Ready"],
            progress.Reports.Select((SandboxProgress p) => p.Description)
        );
    }

    [Fact]
    public async Task ProvisionScenarioAsync_InGerman_ReportsTheProgressPhasesInGerman()
    {
        using UiCulture culture = UiCulture.Use("de");
        await using SandboxDatabase database = Create();
        RecordingProgress progress = new RecordingProgress();

        await database.ProvisionScenarioAsync(ScenarioId, progress);

        Assert.Equal(
            [
                "Datenbank wird zurückgesetzt",
                "Data/one/Schema/01.sql wird ausgeführt",
                "Data/one/Seed/01.sql wird ausgeführt",
                "Datenbankschema wird gelesen",
                "Bereit",
            ],
            progress.Reports.Select((SandboxProgress p) => p.Description)
        );
    }

    [Fact]
    public async Task ProvisionScenarioAsync_UnknownScenario_ListsTheAvailableOnesInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();

        ProvisionResult result = await database.ProvisionScenarioAsync("nope");

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.UnknownScenario, error.Kind);
        Assert.Equal("The scenario \"nope\" is unknown. Available scenarios: one.", error.Message);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_CancelledToken_ReportsTheEnglishCancellation()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        ProvisionResult result = await database.ProvisionScenarioAsync(
            ScenarioId,
            null,
            source.Token
        );

        Assert.Equal(ProvisioningErrorKind.Cancelled, result.Error?.Kind);
        Assert.Equal("Loading the scenario was cancelled.", result.Error?.Message);
    }

    [Fact]
    public async Task ProvisionScenarioAsync_FailingScript_NamesTheScriptInEnglishAndKeepsTheMsSqlMessage()
    {
        using UiCulture culture = UiCulture.Use("en");
        await using SandboxDatabase database = SandboxFactory.Create(
            catalog: CatalogWith(
                "CREATE TABLE Dup (Id INTEGER PRIMARY KEY);",
                "INSERT INTO Dup VALUES (1); INSERT INTO Dup VALUES (1);"
            )
        );

        ProvisionResult result = await database.ProvisionScenarioAsync(ScenarioId);

        ProvisioningError error = Assert.IsType<ProvisioningError>(result.Error);
        Assert.Equal(ProvisioningErrorKind.ScriptFailed, error.Kind);
        Assert.StartsWith(
            "The script Data/one/Seed/01.sql could not be run: Violation of PRIMARY KEY constraint",
            error.Message
        );
    }

    [Fact]
    public async Task ExecuteAsync_SqlError_KeepsTheOriginalMsSqlMessageInGerman()
    {
        using UiCulture culture = UiCulture.Use("de");
        await using SandboxDatabase database = Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT * FROM Missing");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Sql);
        Assert.Equal(208, error.MsSql?.Number);
        Assert.Contains("Invalid object name 'Missing'.", error.Display);
    }
}

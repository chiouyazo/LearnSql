using LearnSql.Data.Scenarios;
using LearnSql.Sql.Execution;
using LearnSql.Sql.Models;
using LearnSql.Sql.Translation;
using LearnSql.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LearnSql.Tests.Execution;

public class ExecutionContractTests
{
    public static TheoryData<string> HostileScripts() =>
        new TheoryData<string>
        {
            "",
            "   \n\t  ",
            "SELECT",
            "SELECT 1 FROM",
            "SELECT * FROM DoesNotExist",
            "DROP TABLE DoesNotExist",
            "INSERT INTO DoesNotExist VALUES (1)",
            "GO",
            "GO 5",
            "'unterminated",
            "SELECT 1; GO 0",
            "\u0000",
            "SELECT '\u0000'",
            "PRAGMA writable_schema = 1",
            "VACUUM",
            "ATTACH DATABASE 'other.db' AS other",
            "SELECT load_extension('nothing')",
            "DROP VIEW COLUMNS",
            "COMMIT",
            "ROLLBACK",
            "SELECT 1; SELECT FROM",
            new string('x', 100_000),
        };

    public static TheoryData<int, long> InvalidLimits() =>
        new TheoryData<int, long>
        {
            { 0, 1000 },
            { -1, 1000 },
            { int.MinValue, 1000 },
            { 10, 0 },
            { 10, -5 },
            { 10, -2 },
        };

    [Fact]
    public async Task ExecuteAsync_NullScript_ThrowsArgumentNullException()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        await Assert.ThrowsAsync<ArgumentNullException>(() => database.ExecuteAsync(null!));
    }

    [Theory]
    [MemberData(nameof(InvalidLimits))]
    public async Task ExecuteAsync_InvalidOptions_ThrowsArgumentOutOfRangeException(
        int maxRows,
        long timeoutMilliseconds
    )
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions
        {
            MaxRowsPerResultSet = maxRows,
            Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds),
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            database.ExecuteAsync("SELECT 1", options)
        );
    }

    [Fact]
    public async Task ExecuteAsync_InfiniteTimeout_IsAValidOption()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { Timeout = Timeout.InfiniteTimeSpan };

        ExecutionResult result = await database.ExecuteAsync("SELECT 1", options);

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
    }

    [Fact]
    public async Task ExecuteAsync_InvalidOptions_DoNotDisturbTheDatabase()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT); INSERT INTO T VALUES (1)");
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 0 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            database.ExecuteAsync("DELETE FROM T", options)
        );

        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Theory]
    [MemberData(nameof(HostileScripts))]
    public async Task ExecuteAsync_HostileScript_ReturnsAResultInsteadOfThrowing(string script)
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(script, SandboxFactory.Timeboxed());

        Assert.NotNull(result);
        Assert.Equal(result.Error is null, result.Succeeded);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Theory]
    [MemberData(nameof(HostileScripts))]
    public async Task ExecuteAsync_HostileScript_LeavesTheDatabaseUsable(string script)
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteAsync(script, SandboxFactory.Timeboxed());

        object? value = await database.ScalarAsync("SELECT 41 + 1");

        Assert.Equal(42L, value);
    }

    [Fact]
    public async Task ExecuteAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => database.ExecuteAsync("SELECT 1"));
    }

    [Fact]
    public async Task OtherOperations_AfterDispose_ThrowObjectDisposedException()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => database.GetSchemaAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => database.InitializeAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            database.ProvisionScenarioAsync("starter")
        );
        await Assert.ThrowsAsync<ObjectDisposedException>(() => database.ResetAsync());
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNothingTheSecondTime()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("SELECT 1");

        await database.DisposeAsync();
        Exception? exception = await Record.ExceptionAsync(async () =>
            await database.DisposeAsync()
        );

        Assert.Null(exception);
        Assert.False(database.Status.IsInitialized);
    }

    [Fact]
    public async Task ExecuteAsync_Success_ReportsNoErrorAndAPositiveDuration()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1");

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
        Assert.False(result.RolledBack);
        Assert.False(result.Truncated);
        Assert.True(result.Duration > TimeSpan.Zero);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyScript_SucceedsWithoutStatements()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("   ");

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.Empty(result.Statements);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_FirstCall_InitializesTheDatabaseOnDemand()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        Assert.False(database.Status.IsInitialized);

        await database.ExecuteOkAsync("SELECT 1");

        Assert.True(database.Status.IsInitialized);
        Assert.Null(database.Status.ScenarioId);
        Assert.False(database.Status.IsBusy);
    }

    [Fact]
    public void Constructor_NullArguments_ThrowArgumentNullException()
    {
        ScenarioCatalog catalog = SqliteTestDatabase.ScenarioCatalog;
        IErrorTranslator translator = ErrorTranslatorFactory.Create(Language.MsSql);
        IStatementPolicy[] policies = [new SystemObjectPolicy()];
        ICooperativeYield yield = new TimeSliceYield(TimeSpan.Zero);
        SandboxOptions options = new SandboxOptions();
        TimeProvider time = TimeProvider.System;
        NullLogger<SandboxDatabase> logger = NullLogger<SandboxDatabase>.Instance;

        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(null!, translator, policies, yield, options, time, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, null!, policies, yield, options, time, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, translator, null!, yield, options, time, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, translator, policies, null!, options, time, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, translator, policies, yield, null!, time, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, translator, policies, yield, options, null!, logger)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SandboxDatabase(catalog, translator, policies, yield, options, time, null!)
        );
    }

    [Fact]
    public void Constructor_InvalidSandboxOptions_ThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SandboxFactory.Create(options: new SandboxOptions { RowsPerYieldCheck = 0 })
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SandboxFactory.Create(options: new SandboxOptions { MaxDatabaseBytes = 0 })
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SandboxFactory.Create(options: new SandboxOptions { ProgressHandlerInstructions = -1 })
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SandboxFactory.Create(options: new SandboxOptions { MaxValueBytes = 0 })
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SandboxFactory.Create(
                options: new SandboxOptions { YieldInterval = TimeSpan.FromSeconds(-1) }
            )
        );
    }

    [Fact]
    public async Task CreateDefault_ProducesAWorkingSandbox()
    {
        await using SandboxDatabase database = SandboxDatabase.CreateDefault();

        Assert.Equal(7L, await database.ScalarAsync("SELECT 3 + 4"));
    }

    [Fact]
    public async Task AddSandboxDatabase_RegistersAResolvableSingleton()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddSandboxDatabase();
        await using ServiceProvider provider = services.BuildServiceProvider();

        ISandboxDatabase first = provider.GetRequiredService<ISandboxDatabase>();
        ISandboxDatabase second = provider.GetRequiredService<ISandboxDatabase>();

        Assert.Same(first, second);
        Assert.Equal(2L, await first.ScalarAsync("SELECT 1 + 1"));
    }
}

using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Execution;
using LearnSql.Sql.Models;
using LearnSql.Sql.Translation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LearnSql.Tests.Support;

public static class SandboxFactory
{
    public static readonly TimeSpan ShortTimeout = TimeSpan.FromMilliseconds(300);

    public static readonly string EndlessRecursiveQuery =
        "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r) SELECT COUNT(*) FROM r";

    public static SandboxDatabase Create(
        ScenarioCatalog? catalog = null,
        IEnumerable<IStatementPolicy>? policies = null,
        ILogger<SandboxDatabase>? logger = null,
        SandboxOptions? options = null,
        TimeProvider? timeProvider = null,
        ICooperativeYield? cooperativeYield = null
    )
    {
        SandboxOptions effective = options ?? new SandboxOptions();
        return new SandboxDatabase(
            catalog ?? SqliteTestDatabase.ScenarioCatalog,
            ErrorTranslatorFactory.Create(Language.MsSql),
            policies ?? [new SystemObjectPolicy()],
            cooperativeYield ?? new TimeSliceYield(effective.YieldInterval),
            effective,
            timeProvider ?? TimeProvider.System,
            logger ?? NullLogger<SandboxDatabase>.Instance
        );
    }

    public static async Task<SandboxDatabase> CreateProvisionedAsync(string scenarioId)
    {
        SandboxDatabase database = Create();
        ProvisionResult result = await database.ProvisionScenarioAsync(scenarioId);
        Assert.True(result.Succeeded, result.Error?.Message);
        return database;
    }

    public static ExecutionOptions Timeboxed(bool rollback = false)
    {
        return new ExecutionOptions { Timeout = ShortTimeout, RollbackAfterExecution = rollback };
    }

    public static ExecutionOptions Rollback()
    {
        return new ExecutionOptions { RollbackAfterExecution = true };
    }

    public static async Task<ExecutionResult> ExecuteOkAsync(
        this ISandboxDatabase database,
        string tsql,
        ExecutionOptions? options = null
    )
    {
        ExecutionResult result = await database.ExecuteAsync(tsql, options);
        Assert.True(result.Succeeded, Describe(result));
        return result;
    }

    public static async Task<object?> ScalarAsync(this ISandboxDatabase database, string tsql)
    {
        ExecutionResult result = await database.ExecuteOkAsync(tsql);
        return Assert.Single(Assert.Single(result.ResultSets).Rows)[0];
    }

    public static async Task<long> CountRowsAsync(this ISandboxDatabase database, string table)
    {
        object? value = await database.ScalarAsync($"SELECT COUNT(*) FROM {table}");
        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static Task<ExecutionResult> ExecuteOnWorkerAsync(
        this ISandboxDatabase database,
        string tsql,
        ExecutionOptions? options = null
    )
    {
        return Task.Run(() => database.ExecuteAsync(tsql, options));
    }

    public static async Task WaitUntilBusyAsync(this ISandboxDatabase database)
    {
        DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (!database.Status.IsBusy)
        {
            Assert.True(DateTime.UtcNow < deadline, "The database never became busy.");
            await Task.Delay(2);
        }
    }

    public static ExecutionError ExpectError(this ExecutionResult result, ExecutionErrorKind kind)
    {
        Assert.NotNull(result.Error);
        Assert.Equal(kind, result.Error.Kind);
        return result.Error;
    }

    public static string Describe(ExecutionResult result)
    {
        if (result.Error is null)
        {
            return "Succeeded.";
        }

        string diagnostics = string.Join(
            "; ",
            result.Diagnostics.Select((DialectDiagnostic d) => d.Message)
        );
        return $"{result.Error.Kind}: {result.Error.Message} {result.Error.Detail} {diagnostics}";
    }
}

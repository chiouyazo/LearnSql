using LearnSql.Data.Scenarios;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Execution;

public class ScenarioWalkthroughTests : IClassFixture<ScenarioSandboxFixture>
{
    private readonly ScenarioSandboxFixture _fixture;

    public ScenarioWalkthroughTests(ScenarioSandboxFixture fixture)
    {
        _fixture = fixture;
    }

    public static TheoryData<string, string> ExerciseKeys() => ScenarioData.ExerciseKeys();

    public static TheoryData<string, string> GuidedKeys() => ScenarioData.GuidedKeys();

    private static long TotalRowsAffected(ExecutionResult result)
    {
        return result.Statements.Sum((StatementResult s) => (long)(s.RowsAffected ?? 0));
    }

    private static async Task<string> FingerprintAsync(ISandboxDatabase database)
    {
        DatabaseSchema schema = await database.GetSchemaAsync();
        List<string> parts = [];
        foreach (RelationSchema table in schema.Tables.Where((RelationSchema t) => !t.IsSystem))
        {
            long rows = await database.CountRowsAsync(table.Name);
            parts.Add($"{table.Name}:{table.Columns.Count}:{rows}");
        }

        parts.AddRange(
            schema
                .Views.Where((RelationSchema v) => !v.IsSystem)
                .Select((RelationSchema v) => "view:" + v.Name)
        );
        return string.Join("|", parts.Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task ExerciseSolution_ProducesTheExpectedResultThroughTheSandbox(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SandboxDatabase database = await _fixture.GetAsync(scenarioId);

        ExecutionResult result = await database.ExecuteAsync(exercise.SolutionSql);

        Assert.True(result.Succeeded, $"{exerciseId}: {SandboxFactory.Describe(result)}");
        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.False(resultSet.Truncated);
        Assert.Equal(exercise.ExpectedRowCount, resultSet.Rows.Count);
        Assert.NotEmpty(resultSet.Columns);
        Assert.False(result.RolledBack);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task ExerciseSolution_InRollbackMode_ProducesTheSameRows(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SandboxDatabase database = await _fixture.GetAsync(scenarioId);

        ExecutionResult normal = await database.ExecuteAsync(exercise.SolutionSql);
        ExecutionResult rolledBack = await database.ExecuteAsync(
            exercise.SolutionSql,
            SandboxFactory.Rollback()
        );

        Assert.True(rolledBack.Succeeded, SandboxFactory.Describe(rolledBack));
        Assert.True(rolledBack.RolledBack);
        Assert.Equal(
            Flatten(Assert.Single(normal.ResultSets)),
            Flatten(Assert.Single(rolledBack.ResultSets))
        );
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task ExerciseSolution_DoesNotChangeTheDatabase(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SandboxDatabase database = await _fixture.GetAsync(scenarioId);
        string before = await FingerprintAsync(database);

        await database.ExecuteOkAsync(exercise.SolutionSql);

        Assert.Equal(before, await FingerprintAsync(database));
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public async Task GuidedSteps_MeetTheirExpectationsThroughTheSandbox(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            scenarioId
        );

        foreach (GuidedStep step in guided.Steps)
        {
            string before = await FingerprintAsync(database);

            ExecutionResult result = await database.ExecuteAsync(step.Sql);

            if (step.ExpectsError)
            {
                ExecutionError error = result.ExpectError(ExecutionErrorKind.Sql);
                Assert.NotNull(error.MsSql);
                Assert.Equal(before, await FingerprintAsync(database));
                continue;
            }

            Assert.True(result.Succeeded, $"{step.Id}: {SandboxFactory.Describe(result)}");
            if (step.ExpectedResultRows is int rows)
            {
                Assert.NotEmpty(result.ResultSets);
                Assert.Equal(rows, result.ResultSets[^1].Rows.Count);
                Assert.False(result.Truncated);
            }

            if (step.ExpectedRowsAffected is int affected)
            {
                Assert.Equal(affected, TotalRowsAffected(result));
            }

            if (step.Kind == GuidedStepKind.ReadOnly)
            {
                Assert.Equal(before, await FingerprintAsync(database));
            }
        }
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public async Task GuidedSteps_InRollbackMode_LeaveTheDatabaseUntouched(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            scenarioId
        );
        string before = await FingerprintAsync(database);

        foreach (GuidedStep step in guided.Steps)
        {
            await database.ExecuteAsync(step.Sql, SandboxFactory.Rollback());

            Assert.Equal(before, await FingerprintAsync(database));
        }
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public async Task GuidedScenario_RunsIdenticallyAfterAReset(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            scenarioId
        );

        List<bool> firstRun = [];
        foreach (GuidedStep step in guided.Steps)
        {
            firstRun.Add((await database.ExecuteAsync(step.Sql)).Succeeded);
        }

        await database.ResetAsync();
        List<bool> secondRun = [];
        foreach (GuidedStep step in guided.Steps)
        {
            secondRun.Add((await database.ExecuteAsync(step.Sql)).Succeeded);
        }

        Assert.Equal(firstRun, secondRun);
    }

    private static string Flatten(SqlResultSet resultSet)
    {
        IEnumerable<string> rows = resultSet.Rows.Select(
            (object?[] row) =>
                string.Join(
                    "|",
                    row.Select(
                        (object? value) =>
                            value is null
                                ? "NULL"
                                : Convert.ToString(
                                    value,
                                    System.Globalization.CultureInfo.InvariantCulture
                                ) ?? string.Empty
                    )
                )
        );
        return string.Join("\n", rows);
    }
}

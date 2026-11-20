using LearnSql.Core.Services.Exercises;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Core;

public sealed class ExerciseScenarioDatabases : IAsyncLifetime
{
    private readonly Dictionary<string, SandboxDatabase> _databases = new Dictionary<
        string,
        SandboxDatabase
    >(StringComparer.Ordinal);

    public async Task<SandboxDatabase> GetAsync(string scenarioId)
    {
        if (_databases.TryGetValue(scenarioId, out SandboxDatabase? existing))
        {
            return existing;
        }

        SandboxDatabase created = SandboxDatabase.CreateDefault();
        ProvisionResult result = await created.ProvisionScenarioAsync(scenarioId);
        Assert.True(result.Succeeded, result.Error?.Message);
        _databases[scenarioId] = created;
        return created;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (SandboxDatabase database in _databases.Values)
        {
            await database.DisposeAsync();
        }
    }
}

public sealed class ExerciseDataTests : IClassFixture<ExerciseScenarioDatabases>
{
    private static readonly ExecutionOptions Isolated = new ExecutionOptions
    {
        RollbackAfterExecution = true,
    };

    private readonly ExerciseScenarioDatabases _databases;

    public ExerciseDataTests(ExerciseScenarioDatabases databases)
    {
        _databases = databases;
    }

    public static TheoryData<string, string> ExerciseKeys() => ScenarioData.ExerciseKeys();

    private async Task<SqlResultSet> RunSolutionAsync(
        string scenarioId,
        ExerciseDefinition exercise
    )
    {
        SandboxDatabase database = await _databases.GetAsync(scenarioId);
        ExecutionResult result = await database.ExecuteAsync(exercise.SolutionSql, Isolated);
        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        return Assert.Single(result.ResultSets);
    }

    private static SqlResultSet WithRows(SqlResultSet source, IEnumerable<object?[]> rows)
    {
        return new SqlResultSet(source.Columns, [.. rows], false);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_AgainstTheRealSandbox_IsEqualToItselfAndMatchesTheExpectedRowCount(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);

        SqlResultSet first = await RunSolutionAsync(scenarioId, exercise);
        SqlResultSet second = await RunSolutionAsync(scenarioId, exercise);
        ResultComparison comparison = ResultComparer.Compare(first, second, exercise.OrderMatters);

        Assert.True(comparison.IsEqual);
        Assert.Equal(exercise.ExpectedRowCount, first.Rows.Count);
        Assert.False(first.Truncated);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_WithAMissingRow_IsDetectedAsDifferent(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SqlResultSet expected = await RunSolutionAsync(scenarioId, exercise);
        SqlResultSet actual = WithRows(expected, expected.Rows.Take(expected.Rows.Count - 1));

        ResultComparison comparison = ResultComparer.Compare(
            expected,
            actual,
            exercise.OrderMatters
        );

        Assert.False(comparison.IsEqual);
        Assert.Single(comparison.MissingRows);
        Assert.Empty(comparison.ExtraRows);
        Assert.True(comparison.RowCountDiffers);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_WithAnExtraRow_IsDetectedAsDifferent(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SqlResultSet expected = await RunSolutionAsync(scenarioId, exercise);
        object?[] stray = [.. expected.Rows[0].Select((object? _) => (object?)"stray")];
        SqlResultSet actual = WithRows(expected, expected.Rows.Append(stray));

        ResultComparison comparison = ResultComparer.Compare(
            expected,
            actual,
            exercise.OrderMatters
        );

        Assert.False(comparison.IsEqual);
        Assert.Empty(comparison.MissingRows);
        Assert.Single(comparison.ExtraRows);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_WithOneChangedValue_IsDetectedAsDifferent(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SqlResultSet expected = await RunSolutionAsync(scenarioId, exercise);
        object?[] changed = [.. expected.Rows[0]];
        changed[0] = changed[0] switch
        {
            null => "changed",
            long number => number + 1,
            double number => number + 1,
            string text => text + "x",
            _ => "changed",
        };
        SqlResultSet actual = WithRows(expected, expected.Rows.Skip(1).Prepend(changed));

        ResultComparison comparison = ResultComparer.Compare(
            expected,
            actual,
            exercise.OrderMatters
        );

        Assert.False(comparison.IsEqual);
        Assert.Single(comparison.MissingRows);
        Assert.Single(comparison.ExtraRows);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_WithADroppedColumn_IsDetectedAsDifferent(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SqlResultSet expected = await RunSolutionAsync(scenarioId, exercise);
        SqlResultSet actual = new SqlResultSet(
            [.. expected.Columns.Skip(1)],
            [.. expected.Rows.Select((object?[] row) => row[1..])],
            false
        );

        ResultComparison comparison = ResultComparer.Compare(
            expected,
            actual,
            exercise.OrderMatters
        );

        Assert.False(comparison.IsEqual);
        Assert.True(comparison.ColumnCountDiffers);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task Solution_WithReversedRows_IsOnlyWrongWhenTheOrderMatters(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        SqlResultSet expected = await RunSolutionAsync(scenarioId, exercise);
        SqlResultSet actual = WithRows(expected, expected.Rows.Reverse());
        bool reversalChangesTheSequence = !ResultComparer.Compare(expected, actual, true).IsEqual;

        ResultComparison comparison = ResultComparer.Compare(
            expected,
            actual,
            exercise.OrderMatters
        );

        Assert.Equal(!(exercise.OrderMatters && reversalChangesTheSequence), comparison.IsEqual);
        if (exercise.OrderMatters && reversalChangesTheSequence)
        {
            Assert.True(comparison.OrderDiffers);
        }
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public async Task OrderedSolution_HasAnUnambiguousRowSequence(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        if (!exercise.OrderMatters)
        {
            return;
        }

        SandboxDatabase database = await _databases.GetAsync(scenarioId);
        SqlResultSet baseline = await RunSolutionAsync(scenarioId, exercise);
        string statement = exercise.SolutionSql.Trim().TrimEnd(';');
        IEnumerable<int> ordinals = Enumerable.Range(1, baseline.Columns.Count);
        string ascending =
            statement
            + ", "
            + string.Join(", ", ordinals.Select((int ordinal) => ordinal + " ASC"));
        string descending =
            statement
            + ", "
            + string.Join(", ", ordinals.Select((int ordinal) => ordinal + " DESC"));

        ExecutionResult ascendingResult = await database.ExecuteAsync(ascending, Isolated);
        ExecutionResult descendingResult = await database.ExecuteAsync(descending, Isolated);

        Assert.True(ascendingResult.Succeeded, SandboxFactory.Describe(ascendingResult));
        Assert.True(descendingResult.Succeeded, SandboxFactory.Describe(descendingResult));
        SqlResultSet ascendingSet = Assert.Single(ascendingResult.ResultSets);
        SqlResultSet descendingSet = Assert.Single(descendingResult.ResultSets);
        Assert.True(
            ResultComparer.Compare(ascendingSet, descendingSet, true).IsEqual,
            "Rows that tie on the ORDER BY keys appear in an arbitrary order."
        );
        Assert.True(ResultComparer.Compare(baseline, ascendingSet, true).IsEqual);
    }

    [Fact]
    public async Task OrderedExercises_WithAGenuinelySwappedPair_AreRejectedForAtLeastThirtyExercises()
    {
        int rejected = 0;
        foreach (ScenarioDefinition scenario in ScenarioData.Catalog.Scenarios)
        {
            foreach (
                ExerciseDefinition exercise in scenario.Exercises.Where(
                    (ExerciseDefinition e) => e.OrderMatters && e.ExpectedRowCount > 1
                )
            )
            {
                SqlResultSet expected = await RunSolutionAsync(scenario.Id, exercise);
                object?[][] rows = [.. expected.Rows];
                (rows[0], rows[^1]) = (rows[^1], rows[0]);
                SqlResultSet swapped = WithRows(expected, rows);

                ResultComparison comparison = ResultComparer.Compare(expected, swapped, true);

                Assert.True(ResultComparer.Compare(expected, swapped, false).IsEqual);
                if (!comparison.IsEqual)
                {
                    Assert.True(comparison.OrderDiffers);
                    rejected++;
                }
            }
        }

        Assert.True(rejected >= 30, "Only " + rejected + " swapped results were rejected.");
    }
}

using System.Text.RegularExpressions;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect;
using LearnSql.Tests.Support;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Tests.Scenarios;

public partial class ExerciseTests
{
    public static TheoryData<string, string> ExerciseKeys() => ScenarioData.ExerciseKeys();

    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    [GeneratedRegex("[\u2013\u2014]")]
    private static partial Regex DashPattern();

    private static bool HasTopLevelOrderBy(string sql)
    {
        TSqlParser parser = TSqlParser.CreateParser(SqlVersion.Sql180, true);
        using StringReader reader = new StringReader(sql);
        TSqlFragment fragment = parser.Parse(reader, out IList<ParseError> errors);
        Assert.Empty(errors);

        TSqlStatement statement = ((TSqlScript)fragment).Batches[0].Statements[0];
        QueryExpression query = Assert.IsType<SelectStatement>(statement).QueryExpression;
        return query switch
        {
            QuerySpecification specification => specification.OrderByClause is not null,
            BinaryQueryExpression binary => binary.OrderByClause is not null,
            QueryParenthesisExpression parenthesis => parenthesis.OrderByClause is not null,
            _ => false,
        };
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void SolutionSql_RunsThroughTheDialectPipelineWithoutError(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);

        ScriptOutcome outcome = database.Run(exercise.SolutionSql);

        Assert.True(outcome.Succeeded, outcome.Describe());
        Assert.Single(outcome.ResultSets);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void SolutionSql_ReturnsTheExpectedNumberOfRows(string scenarioId, string exerciseId)
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);

        ResultSet result = database.Query(exercise.SolutionSql);

        Assert.Equal(exercise.ExpectedRowCount, result.RowCount);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void SolutionSql_WithOrderMatters_SortsTheOutermostQuery(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);

        if (exercise.OrderMatters)
            Assert.True(
                HasTopLevelOrderBy(exercise.SolutionSql),
                "OrderMatters requires an ORDER BY."
            );
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void SolutionSql_IsDeterministicAcrossFreshDatabases(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        using SqliteTestDatabase first = SqliteTestDatabase.ForScenario(scenarioId);
        using SqliteTestDatabase second = SqliteTestDatabase.ForScenario(scenarioId);

        ResultSet left = first.Query(exercise.SolutionSql);
        ResultSet right = second.Query(exercise.SolutionSql);

        Assert.Equal(left.Columns, right.Columns);
        if (exercise.OrderMatters)
            Assert.Equal(left.Rows.Select(Flatten), right.Rows.Select(Flatten));
        else
            Assert.Equal(left.Rows.Select(Flatten).Order(), right.Rows.Select(Flatten).Order());
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void SolutionSql_DoesNotChangeTheDatabase(string scenarioId, string exerciseId)
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);
        string before = database.StateFingerprint();

        database.RunSuccessfully(exercise.SolutionSql);

        Assert.Equal(before, database.StateFingerprint());
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void Exercise_HasTextsAndAtLeastThreeNonEmptyDistinctHints(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);

        Assert.False(string.IsNullOrWhiteSpace(exercise.Title));
        Assert.False(string.IsNullOrWhiteSpace(exercise.Description));
        Assert.False(string.IsNullOrWhiteSpace(exercise.SolutionSql));
        Assert.True(exercise.Hints.Count >= 3, "Hint levels are missing.");
        Assert.All(exercise.Hints, (string hint) => Assert.False(string.IsNullOrWhiteSpace(hint)));
        Assert.Equal(exercise.Hints.Count, exercise.Hints.Distinct().Count());
        Assert.True(exercise.ExpectedRowCount >= 0);
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void Exercise_FirstHintDoesNotGiveAwayTheSolution(string scenarioId, string exerciseId)
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);

        Assert.DoesNotContain(
            exercise.SolutionSql.TrimEnd(';'),
            exercise.Hints[0],
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void Exercise_UserVisibleTexts_ContainNoEmDashOrEnDash(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        string all = string.Join(
            "\n",
            new[] { exercise.Title, exercise.Description }.Concat(exercise.Hints)
        );

        Assert.DoesNotMatch(DashPattern(), all);
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_ExerciseIdsAreUnique(string scenarioId)
    {
        IReadOnlyList<ExerciseDefinition> exercises = ScenarioData
            .Catalog.GetScenario(scenarioId)
            .Exercises;

        Assert.Equal(
            exercises.Count,
            exercises
                .Select((ExerciseDefinition e) => e.Id)
                .Distinct(StringComparer.Ordinal)
                .Count()
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_CoversBeginnerToAdvancedExercises(string scenarioId)
    {
        IReadOnlyList<ExerciseDefinition> exercises = ScenarioData
            .Catalog.GetScenario(scenarioId)
            .Exercises;

        Assert.Contains(
            exercises,
            (ExerciseDefinition e) => e.Difficulty == ExerciseDifficulty.Beginner
        );
        Assert.Contains(
            exercises,
            (ExerciseDefinition e) => e.Difficulty == ExerciseDifficulty.Advanced
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_ScenarioTextsContainNoEmDashOrEnDash(string scenarioId)
    {
        ScenarioDefinition scenario = ScenarioData.Catalog.GetScenario(scenarioId);

        Assert.DoesNotMatch(DashPattern(), scenario.Title + "\n" + scenario.Description);
    }

    private static string Flatten(IReadOnlyList<object?> row)
    {
        return string.Join(
            "|",
            row.Select(
                (object? value) =>
                    value is null
                        ? "NULL"
                        : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
            )
        );
    }
}

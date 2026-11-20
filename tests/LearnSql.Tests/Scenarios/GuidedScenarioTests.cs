using System.Text.RegularExpressions;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public partial class GuidedScenarioTests
{
    private const string Ellipsis = "...";

    public static TheoryData<string, string> GuidedKeys() => ScenarioData.GuidedKeys();

    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    [GeneratedRegex("[\u2013\u2014]")]
    private static partial Regex DashPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();

    private static string NormalizeWhitespace(string text)
    {
        return WhitespacePattern().Replace(text, " ").Replace("( ", "(").Replace(" )", ")");
    }

    private static bool ContainsInOrder(string sql, string fragment)
    {
        string normalizedSql = NormalizeWhitespace(sql);
        string[] segments = fragment.Split(Ellipsis, StringSplitOptions.RemoveEmptyEntries);
        int position = 0;
        foreach (string segment in segments)
        {
            string normalizedSegment = NormalizeWhitespace(segment);
            int index = normalizedSql.IndexOf(
                normalizedSegment,
                position,
                StringComparison.Ordinal
            );
            if (index < 0)
                return false;
            position = index + normalizedSegment.Length;
        }

        return segments.Length > 0;
    }

    private static IEnumerable<string> TextsOf(GuidedScenarioDefinition guided)
    {
        yield return guided.Title;
        yield return guided.Description;
        foreach (GuidedStep step in guided.Steps)
        {
            yield return step.Title;
            yield return step.ResultExplanation;
            yield return step.DatabaseEffect;
            yield return step.QueryExplanation.Summary;
            foreach (SqlPartExplanation part in step.QueryExplanation.Parts)
            {
                yield return part.Fragment;
                yield return part.Explanation;
            }
        }
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_RunInOrderOnAFreshDatabaseAndMatchTheirExpectations(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);

        foreach (GuidedStep step in guided.Steps)
        {
            string stateBefore = database.StateFingerprint();

            ScriptOutcome outcome = database.Run(step.Sql);

            if (step.ExpectsError)
            {
                Assert.True(outcome.TranslationSucceeded, $"{step.Id}: {outcome.Describe()}");
                Assert.True(outcome.Error is not null, $"{step.Id} should fail but succeeded.");
                Assert.Equal(stateBefore, database.StateFingerprint());
                continue;
            }

            Assert.True(outcome.Succeeded, $"{step.Id}: {outcome.Describe()}");
            if (step.ExpectedResultRows is int rows)
            {
                Assert.True(outcome.ResultSets.Count > 0, $"{step.Id} returned no result set.");
                Assert.Equal(rows, outcome.LastResultSet.RowCount);
            }

            if (step.ExpectedRowsAffected is int affected)
                Assert.Equal(affected, outcome.RecordsAffected);

            if (step.Kind == GuidedStepKind.ReadOnly)
                Assert.Equal(stateBefore, database.StateFingerprint());
            else
                Assert.NotEqual(stateBefore, database.StateFingerprint());
        }
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_LeaveNoForeignKeyViolations(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);

        foreach (GuidedStep step in guided.Steps)
            database.Run(step.Sql);

        Assert.Empty(database.ForeignKeyViolations());
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_AreRepeatableOnAnotherFreshDatabase(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        for (int run = 0; run < 2; run++)
        {
            using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(scenarioId);
            foreach (GuidedStep step in guided.Steps)
            {
                ScriptOutcome outcome = database.Run(step.Sql);
                Assert.Equal(!step.ExpectsError, outcome.Succeeded);
            }
        }
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_WhoseKindChangesData_RequireAFreshDatabaseOtherwiseTheyAreReadOnly(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        bool changesData = guided.Steps.Any((GuidedStep s) => s.Kind == GuidedStepKind.ChangesData);

        Assert.Equal(changesData, guided.RequiresFreshDatabase);
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_ThatExpectResultRowsAreReadOnlyAndNeverExpectAnError(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        Assert.All(
            guided.Steps.Where((GuidedStep s) => s.ExpectedResultRows is not null),
            (GuidedStep step) =>
            {
                Assert.Equal(GuidedStepKind.ReadOnly, step.Kind);
                Assert.False(step.ExpectsError);
                Assert.Null(step.ExpectedRowsAffected);
            }
        );
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_ReadOnlyOnesDeclareTheirExpectedRowCount(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        Assert.All(
            guided.Steps.Where((GuidedStep s) => s.Kind == GuidedStepKind.ReadOnly),
            (GuidedStep step) => Assert.NotNull(step.ExpectedResultRows)
        );
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Guided_TextsAreNotEmpty(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        Assert.All(
            TextsOf(guided),
            (string text) => Assert.False(string.IsNullOrWhiteSpace(text), "A text is empty.")
        );
        Assert.All(
            guided.Steps,
            (GuidedStep step) =>
            {
                Assert.NotEmpty(step.QueryExplanation.Parts);
                Assert.False(string.IsNullOrWhiteSpace(step.Sql));
            }
        );
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Guided_TextsContainNoEmDashOrEnDash(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        Assert.DoesNotMatch(DashPattern(), string.Join("\n", TextsOf(guided)));
        Assert.All(guided.Steps, (GuidedStep step) => Assert.DoesNotMatch(DashPattern(), step.Sql));
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void QueryExplanation_FragmentsOccurInTheStepSqlInOrderWithEllipsisAsWildcard(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        foreach (GuidedStep step in guided.Steps)
        {
            foreach (SqlPartExplanation part in step.QueryExplanation.Parts)
            {
                Assert.True(
                    ContainsInOrder(step.Sql, part.Fragment),
                    $"Fragment '{part.Fragment}' of {step.Id} does not occur in its SQL."
                );
            }
        }
    }

    [Theory]
    [InlineData("SELECT TOP 5 a FROM T ORDER BY a", "TOP 5 ... ORDER BY a", true)]
    [InlineData("SELECT a FROM T ORDER BY a", "ORDER BY a ... TOP 5", false)]
    [InlineData("INSERT INTO T (a, b) VALUES (1, 2)", "INSERT INTO T (...)", true)]
    [InlineData("SELECT a", "FROM (...) t", false)]
    [InlineData("SELECT a", "...", false)]
    [InlineData("SELECT\n    a,\n    b\nFROM T", "SELECT a, b FROM T", true)]
    [InlineData("SELECT a\nFROM (\n    SELECT 1\n) t", "FROM (...) t", true)]
    [InlineData(
        "RANK() OVER (\n    PARTITION BY a\n    ORDER BY b\n)",
        "OVER (PARTITION BY a ORDER BY b)",
        true
    )]
    public void ContainsInOrder_AppliesTheEllipsisRule(string sql, string fragment, bool expected)
    {
        Assert.Equal(expected, ContainsInOrder(sql, fragment));
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_GuidedAndStepIdsAreUnique(string scenarioId)
    {
        IReadOnlyList<GuidedScenarioDefinition> guided = ScenarioData
            .Catalog.GetScenario(scenarioId)
            .GuidedScenarios;

        Assert.Equal(
            guided.Count,
            guided.Select((GuidedScenarioDefinition g) => g.Id).Distinct().Count()
        );
        Assert.All(
            guided,
            (GuidedScenarioDefinition g) =>
                Assert.Equal(
                    g.Steps.Count,
                    g.Steps.Select((GuidedStep s) => s.Id).Distinct().Count()
                )
        );
    }

    [Fact]
    public void Catalog_StepIdsAreGloballyUniqueAcrossAllGuidedScenarios()
    {
        string[] stepIds =
        [
            .. ScenarioData
                .Catalog.Scenarios.SelectMany((ScenarioDefinition s) => s.GuidedScenarios)
                .SelectMany((GuidedScenarioDefinition g) => g.Steps)
                .Select((GuidedStep s) => s.Id),
        ];

        Assert.Equal(stepIds.Length, stepIds.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Catalog_EveryScenarioOffersGuidedScenarios()
    {
        Assert.All(
            ScenarioData.Catalog.Scenarios,
            (ScenarioDefinition s) => Assert.NotEmpty(s.GuidedScenarios)
        );
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void Steps_StartWithTheScenarioIdAsPrefixInTheirIds(string scenarioId, string guidedId)
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);

        Assert.All(
            guided.Steps,
            (GuidedStep step) =>
                Assert.StartsWith(guided.Id + "-", step.Id, StringComparison.Ordinal)
        );
    }
}

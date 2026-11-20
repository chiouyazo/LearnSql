using System.Text.RegularExpressions;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect;
using LearnSql.SqlFormat;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Formatting;

public partial class ScenarioSqlFormatTests
{
    public static TheoryData<string, string> ExerciseKeys() => ScenarioData.ExerciseKeys();

    public static TheoryData<string, string> GuidedKeys() => ScenarioData.GuidedKeys();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();

    private static string Collapse(string sql)
    {
        return WhitespacePattern().Replace(sql, " ");
    }

    private static IEnumerable<string> AllSql()
    {
        foreach (ScenarioDefinition scenario in ScenarioData.Catalog.Scenarios)
        {
            foreach (ExerciseDefinition exercise in scenario.Exercises)
                yield return exercise.SolutionSql;
            foreach (GuidedScenarioDefinition guided in scenario.GuidedScenarios)
            {
                foreach (GuidedStep step in guided.Steps)
                    yield return step.Sql;
            }
        }
    }

    private static string Signature(ScriptOutcome outcome)
    {
        string results = string.Join(
            "||",
            outcome.ResultSets.Select(
                (ResultSet set) =>
                    string.Join(",", set.Columns) + "#" + string.Join(";", set.Rows.Select(Flatten))
            )
        );
        string translated = string.Join(
            "||",
            outcome.Translation.Batches.SelectMany(
                (TranslatedBatch batch) =>
                    batch.Statements.Select(
                        (TranslatedStatement statement) => Collapse(statement.Sql)
                    )
            )
        );
        string error = outcome.Error?.Message ?? string.Empty;
        return $"{outcome.Succeeded}|{outcome.RecordsAffected}|{error}|{results}|{translated}";
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

    [Fact]
    public void Catalog_SqlTextsAreAlreadyFormatted()
    {
        Assert.All(AllSql(), (string sql) => Assert.Equal(sql, SqlFormatter.Format(sql)));
    }

    [Fact]
    public void Catalog_SqlTextsContainNoTabsCarriageReturnsOrTrailingBlanks()
    {
        Assert.All(
            AllSql(),
            (string sql) =>
            {
                Assert.DoesNotContain('\t', sql);
                Assert.DoesNotContain('\r', sql);
                Assert.All(sql.Split('\n'), (string line) => Assert.Equal(line.TrimEnd(), line));
                Assert.Equal(sql.Trim(), sql);
            }
        );
    }

    [Fact]
    public void Catalog_SqlTextsAreFormattedByTheIdempotentFormatter()
    {
        Assert.All(
            AllSql(),
            (string sql) => Assert.Equal(sql, SqlFormatter.Format(SqlFormatter.Format(sql)))
        );
    }

    [Fact]
    public void Catalog_ManifestFilesNeedNoRewriteByTheTool()
    {
        foreach (string path in ManifestPaths())
        {
            string json = File.ReadAllText(path);

            Assert.True(
                json == ScenarioManifestFormatter.Rewrite(json),
                $"{path} is not formatted. Run: dotnet run --project tools/LearnSql.SqlFormat"
            );
            Assert.DoesNotContain('\r', json);
        }
    }

    [Theory]
    [MemberData(nameof(ExerciseKeys))]
    public void FormattedSolutionSql_BehavesLikeTheCollapsedOriginal(
        string scenarioId,
        string exerciseId
    )
    {
        ExerciseDefinition exercise = ScenarioData.Exercise(scenarioId, exerciseId);
        using SqliteTestDatabase original = SqliteTestDatabase.ForScenario(scenarioId);
        using SqliteTestDatabase formatted = SqliteTestDatabase.ForScenario(scenarioId);

        ScriptOutcome expected = original.Run(Collapse(exercise.SolutionSql));
        ScriptOutcome actual = formatted.Run(SqlFormatter.Format(exercise.SolutionSql));

        Assert.True(expected.Succeeded, expected.Describe());
        Assert.Equal(Signature(expected), Signature(actual));
        Assert.Equal(original.StateFingerprint(), formatted.StateFingerprint());
    }

    [Theory]
    [MemberData(nameof(GuidedKeys))]
    public void FormattedGuidedSteps_BehaveLikeTheCollapsedOriginalsInOrder(
        string scenarioId,
        string guidedId
    )
    {
        GuidedScenarioDefinition guided = ScenarioData.Guided(scenarioId, guidedId);
        using SqliteTestDatabase original = SqliteTestDatabase.ForScenario(scenarioId);
        using SqliteTestDatabase formatted = SqliteTestDatabase.ForScenario(scenarioId);

        foreach (GuidedStep step in guided.Steps)
        {
            ScriptOutcome expected = original.Run(Collapse(step.Sql));
            ScriptOutcome actual = formatted.Run(SqlFormatter.Format(step.Sql));

            Assert.Equal(Signature(expected), Signature(actual));
            Assert.Equal(original.StateFingerprint(), formatted.StateFingerprint());
        }
    }

    private static IEnumerable<string> ManifestPaths()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            directory is not null && !File.Exists(Path.Combine(directory.FullName, "LearnSql.slnx"))
        )
            directory = directory.Parent;
        Assert.NotNull(directory);

        string data = Path.Combine(directory.FullName, "src", "LearnSql.Data", "Data");
        string[] manifests = Directory.GetFiles(data, "scenario.json", SearchOption.AllDirectories);
        Assert.Equal(ScenarioData.Catalog.Scenarios.Count, manifests.Length);
        return manifests;
    }
}

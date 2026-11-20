using LearnSql.SqlFormat;

namespace LearnSql.Tests.Formatting;

public class ScenarioManifestFormatterTests
{
    private const string Manifest = """
        {
          "id": "demo",
          "exercises": [
            {
              "id": "e-1",
              "hints": ["SELECT 1 FROM T", "select * from T"],
              "solutionSql": "select a, b, c from T where x = 1 and y = 2 and z = 3 and w = 4 and v = 5",
              "sql": "keep"
            }
          ],
          "guidedScenarios": [
            {
              "steps": [ { "id": "g-1", "sql": "SELECT 'ä' FROM T", "title": "select 1 from t" } ]
            }
          ]
        }
        """;

    [Fact]
    public void Rewrite_ChangesOnlyTheSqlStrings()
    {
        string rewritten = ScenarioManifestFormatter.Rewrite(Manifest);

        string expected = Manifest.Replace(
            "\"select a, b, c from T where x = 1 and y = 2 and z = 3 and w = 4 and v = 5\"",
            "\"SELECT\\n    a,\\n    b,\\n    c\\nFROM T\\nWHERE x = 1\\n    AND y = 2\\n    AND z = 3\\n    AND w = 4\\n    AND v = 5\""
        );
        Assert.Equal(expected, rewritten);
    }

    [Fact]
    public void Rewrite_KeepsOtherStringsWithSqlLookingTextUntouched()
    {
        string rewritten = ScenarioManifestFormatter.Rewrite(Manifest);

        Assert.Contains("\"select * from T\"", rewritten);
        Assert.Contains("\"title\": \"select 1 from t\"", rewritten);
        Assert.Contains("\"SELECT 'ä' FROM T\"", rewritten);
    }

    [Fact]
    public void Rewrite_IsIdempotent()
    {
        string once = ScenarioManifestFormatter.Rewrite(Manifest);

        Assert.Equal(once, ScenarioManifestFormatter.Rewrite(once));
    }

    [Fact]
    public void Rewrite_EscapesQuotesAndBackslashesInTheFormattedSql()
    {
        string json = "{\"sql\": \"SELECT 'a\\\\b', \\\"Col\\\" FROM T\"}";

        string rewritten = ScenarioManifestFormatter.Rewrite(json);

        Assert.Equal(json, rewritten);
    }

    [Fact]
    public void Rewrite_PropagatesFormatErrors()
    {
        Assert.Throws<FormatException>(() =>
            ScenarioManifestFormatter.Rewrite("{\"sql\": \"SELECT 'x\"}")
        );
    }

    [Fact]
    public void Rewrite_ThrowsForInvalidJson()
    {
        Assert.ThrowsAny<Exception>(() => ScenarioManifestFormatter.Rewrite("{\"sql\": "));
    }
}

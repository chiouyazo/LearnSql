using System.Text.RegularExpressions;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Hints;
using LearnSql.Sql.Dialect.Parsing;
using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Tests.Support;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Tests.Hints;

public class DialectDifferencesTests
{
    private static readonly IReadOnlyList<UnsupportedConstruct> Catalog =
        UnsupportedConstructs.CreateDefault();

    private static readonly IReadOnlyList<DialectHintTopic> Topics =
        DialectDifferences.CreateDefault(Catalog);

    private static readonly string[] ExpectedTopicIds =
    [
        "DataTypes",
        "Numbers",
        "DatesAndTimes",
        "Collation",
        "NullBehavior",
        "RowLimiting",
        "Strings",
        "VariablesAndBatches",
        "SchemaObjects",
        "NotYetAvailable",
    ];

    public static TheoryData<string> TopicIds()
    {
        TheoryData<string> data = new TheoryData<string>();
        foreach (DialectHintTopic topic in Topics)
            data.Add(topic.Id);
        return data;
    }

    public static TheoryData<string, string> SandboxResultExamples() =>
        new TheoryData<string, string>
        {
            { "SELECT DATEDIFF(year, '2023-12-31', '2024-01-01')", "1" },
            { "SELECT DATEADD(month, 1, '2024-01-31')", "2024-02-29" },
            { "SELECT DATENAME(weekday, '2024-03-05')", "Tuesday" },
            { "SELECT 7 / 2.0", "3.5" },
            { "SELECT AVG(x) FROM (SELECT 1 AS x UNION ALL SELECT 2) t", "1.5" },
            { "SELECT 0.1 + 0.2", "0.30000000000000004" },
            { "SELECT 2147483647 + 1", "2147483648" },
            { "SELECT CASE WHEN 'a' = 'A' THEN 1 ELSE 0 END", "0" },
            { "SELECT CASE WHEN 'a' = 'a ' THEN 1 ELSE 0 END", "0" },
            { "SELECT CASE WHEN 'Ä' LIKE 'ä' THEN 1 ELSE 0 END", "0" },
            { "SELECT 1 UNION SELECT 2 INTERSECT SELECT 3", "1" },
            { "SELECT 'Nr. ' + CAST(5 AS VARCHAR(10))", "Nr. 5" },
            { "SELECT '2' + '3'", "23" },
            { "SELECT '1' + 1", "2" },
            { "DECLARE @t VARCHAR(3) = 'abcdef'; SELECT @t", "abc" },
            { "DECLARE @i INT = 1; SET @i += 4; SELECT @i", "5" },
            {
                "CREATE TABLE P (Id INT IDENTITY(10, 5) PRIMARY KEY); INSERT INTO P DEFAULT VALUES; SELECT Id FROM P",
                "1"
            },
        };

    [Fact]
    public void CreateDefault_ReturnsTheDocumentedTopicsInOrder()
    {
        Assert.Equal(ExpectedTopicIds, Topics.Select((DialectHintTopic t) => t.Id));
    }

    [Fact]
    public void Topics_HaveUniqueIds()
    {
        Assert.Equal(Topics.Count, Topics.Select((DialectHintTopic t) => t.Id).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(TopicIds))]
    public void Topic_HasTitleSummaryAndPoints(string id)
    {
        DialectHintTopic topic = Topics.Single((DialectHintTopic t) => t.Id == id);

        Assert.False(string.IsNullOrWhiteSpace(topic.Title));
        Assert.False(string.IsNullOrWhiteSpace(topic.Summary));
        Assert.NotEmpty(topic.Points);
        Assert.All(topic.Points, (string point) => Assert.False(string.IsNullOrWhiteSpace(point)));
    }

    [Theory]
    [MemberData(nameof(TopicIds))]
    public void Topic_ExamplesHaveStatementAndBothResults(string id)
    {
        DialectHintTopic topic = Topics.Single((DialectHintTopic t) => t.Id == id);

        Assert.All(
            topic.Examples,
            (DialectHintExample example) =>
            {
                Assert.False(string.IsNullOrWhiteSpace(example.Statement));
                Assert.False(string.IsNullOrWhiteSpace(example.MsSqlResult));
                Assert.False(string.IsNullOrWhiteSpace(example.SandboxResult));
            }
        );
    }

    [Theory]
    [MemberData(nameof(TopicIds))]
    public void Topic_TextsContainNoEmDashOrEnDash(string id)
    {
        DialectHintTopic topic = Topics.Single((DialectHintTopic t) => t.Id == id);
        string all = string.Join(
            "\n",
            new[] { topic.Title, topic.Summary }
                .Concat(topic.Points)
                .Concat(
                    topic.Examples.SelectMany(
                        (DialectHintExample e) =>
                            new[] { e.Statement, e.MsSqlResult, e.SandboxResult }
                    )
                )
        );

        Assert.DoesNotMatch(new Regex("[\u2013\u2014]"), all);
    }

    [Fact]
    public void Topics_ExceptTheLastOne_HaveAtLeastOneExample()
    {
        Assert.All(
            Topics.Where((DialectHintTopic t) => t.Id != "NotYetAvailable"),
            (DialectHintTopic topic) => Assert.NotEmpty(topic.Examples)
        );
    }

    [Theory]
    [MemberData(nameof(TopicIds))]
    public void Topic_ExampleStatements_AreValidTSql(string id)
    {
        DialectHintTopic topic = Topics.Single((DialectHintTopic t) => t.Id == id);

        Assert.All(
            topic.Examples,
            (DialectHintExample example) =>
            {
                IReadOnlyList<DialectDiagnostic> diagnostics = MsSqlValidator.Validate(
                    example.Statement,
                    SqlVersion.Sql180
                );
                Assert.True(
                    diagnostics.Count == 0,
                    $"'{example.Statement}' is not valid T-SQL: {string.Join("; ", diagnostics.Select((DialectDiagnostic d) => d.Message))}"
                );
            }
        );
    }

    [Theory]
    [MemberData(nameof(SandboxResultExamples))]
    public void SandboxResult_OfHintExample_MatchesWhatTheSandboxReallyReturns(
        string statement,
        string expectedSandboxResult
    )
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();

        object? value = database.Scalar(statement);

        Assert.Equal(
            expectedSandboxResult,
            value is null
                ? "NULL"
                : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
        );
    }

    [Fact]
    public void SandboxResultExamples_ListEveryLiteralExampleOfTheCatalog()
    {
        HashSet<string> verified =
        [
            .. SandboxResultExamples().Select((object[] row) => (string)row[0]),
        ];
        string[] literalTopics =
        [
            "DatesAndTimes",
            "Numbers",
            "Collation",
            "Strings",
            "VariablesAndBatches",
            "SchemaObjects",
        ];

        IEnumerable<string> statements = Topics
            .Where((DialectHintTopic t) => literalTopics.Contains(t.Id))
            .SelectMany((DialectHintTopic t) => t.Examples)
            .Select((DialectHintExample e) => e.Statement)
            .Where((string s) => !s.StartsWith("INSERT INTO t "));

        Assert.All(statements, (string statement) => Assert.Contains(statement, verified));
    }

    [Fact]
    public void NotYetAvailable_IsTheLastTopic()
    {
        Assert.Equal("NotYetAvailable", Topics[^1].Id);
    }

    [Fact]
    public void NotYetAvailable_ContainsOnePointPerCatalogEntryInCatalogOrder()
    {
        DialectHintTopic topic = Topics[^1];

        Assert.Equal(
            Catalog.Select((UnsupportedConstruct c) => $"{c.Name}. {c.Alternative}"),
            topic.Points
        );
    }

    [Fact]
    public void NotYetAvailable_HasNoExamples()
    {
        Assert.Empty(Topics[^1].Examples);
    }

    [Fact]
    public void NotYetAvailable_IsGeneratedFromTheGivenCatalogOnly()
    {
        UnsupportedConstruct[] custom =
        [
            UnsupportedConstruct.For<UseStatement>("Use", "Der Wechsel", "Nicht nötig."),
            UnsupportedConstruct.For<PrintStatement>("Print", "Ausgabe", "Verwende SELECT."),
        ];

        IReadOnlyList<DialectHintTopic> topics = DialectDifferences.CreateDefault(custom);

        Assert.Equal(["Der Wechsel. Nicht nötig.", "Ausgabe. Verwende SELECT."], topics[^1].Points);
    }

    [Fact]
    public void NotYetAvailable_WithEmptyCatalog_HasNoPoints()
    {
        IReadOnlyList<DialectHintTopic> topics = DialectDifferences.CreateDefault([]);

        Assert.Empty(topics[^1].Points);
        Assert.Equal(ExpectedTopicIds.Length, topics.Count);
    }

    [Fact]
    public void NotYetAvailable_MentionsEveryCatalogEntryByName()
    {
        string all = string.Join("\n", Topics[^1].Points);

        Assert.All(Catalog, (UnsupportedConstruct c) => Assert.Contains(c.Name, all));
    }
}

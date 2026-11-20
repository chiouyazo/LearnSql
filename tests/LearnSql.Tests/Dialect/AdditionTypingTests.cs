using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class AdditionTypingTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public AdditionTypingTests()
    {
        _database.ExecuteRaw(
            "CREATE TABLE T (Name VARCHAR(10), Qty INT, Stamp DATETIME, Price DECIMAL(8,2));"
                + "INSERT INTO T VALUES ('ab', 2, '2024-01-31 10:00:00.000', 1.5);"
        );
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> LiteralCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT '2' + '3'", "23" },
            { "SELECT '1' + '1'", "11" },
            { "SELECT 'a' + 'b' + 'c'", "abc" },
            { "SELECT '1' + 1", 2L },
            { "SELECT 1 + '1'", 2L },
            { "SELECT 1 + 1", 2L },
            { "SELECT '1.5' + 1", 2.5 },
            { "SELECT 'a' + NULL", null },
            { "SELECT NULL + 'a'", null },
            { "SELECT NULL + 1", null },
            { "SELECT 1 + NULL", null },
            { "SELECT 'a' + '' + 'b'", "ab" },
            { "SELECT 'x' + CAST(5 AS VARCHAR(3))", "x5" },
            { "SELECT CAST(1 AS VARCHAR(3)) + '1'", "11" },
            { "SELECT CONVERT(VARCHAR(5), 1) + '2'", "12" },
            { "SELECT CAST('1' AS INT) + 1", 2L },
            { "SELECT LEFT('abc', 2) + 'x'", "abx" },
            { "SELECT UPPER('ab') + LOWER('CD')", "ABcd" },
            { "SELECT 'a' + (SELECT 'b')", "ab" },
            { "SELECT (SELECT 1) + (SELECT 2)", 3L },
            { "SELECT N'a' + N'b'", "ab" },
            { "SELECT 1 + 2 * 3", 7L },
            { "SELECT 'a' + CASE WHEN 1 = 1 THEN 'b' ELSE 'c' END", "ab" },
        };

    public static TheoryData<string, object?> ColumnCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT Name + 'x' FROM T", "abx" },
            { "SELECT Name + Name FROM T", "abab" },
            { "SELECT 'x' + Name FROM T", "xab" },
            { "SELECT Name + CAST(Qty AS VARCHAR(5)) FROM T", "ab2" },
            { "SELECT Qty + 1 FROM T", 3L },
            { "SELECT Qty + Qty FROM T", 4L },
            { "SELECT Qty + '1' FROM T", 3L },
            { "SELECT Price + 1 FROM T", 2.5 },
            { "SELECT Name + NULL FROM T", null },
            { "SELECT ISNULL(Name, 'z') + 'y' FROM T", "aby" },
            { "SELECT t.Name + 'x' FROM T t", "abx" },
            { "SELECT T.Qty + 1 FROM T", 3L },
            { "SELECT Qty + 1 AS Next FROM T", 3L },
            { "SELECT (Name + 'x') + 'y' FROM T", "abxy" },
        };

    public static TheoryData<string, object?> RelationCases() =>
        new TheoryData<string, object?>
        {
            { "WITH c AS (SELECT 'a' AS x, 1 AS n) SELECT x + 'b' FROM c", "ab" },
            { "WITH c AS (SELECT 'a' AS x, 1 AS n) SELECT n + 1 FROM c", 2L },
            { "WITH c AS (SELECT 'a' AS x, 'b' AS y) SELECT x + y FROM c", "ab" },
            { "WITH c (x, n) AS (SELECT 'a', 1) SELECT x + 'b' FROM c", "ab" },
            { "WITH c (x, n) AS (SELECT 'a', 1) SELECT n + 1 FROM c", 2L },
            { "SELECT d.s + 'x' FROM (SELECT 'a' AS s) d", "ax" },
            { "SELECT d.n + 1 FROM (SELECT 1 AS n) d", 2L },
            { "SELECT s + t FROM (SELECT 'a' AS s, 'b' AS t) d", "ab" },
            { "SELECT d.Name + 'x' FROM (SELECT Name FROM T) d", "abx" },
            { "SELECT d.Qty + 1 FROM (SELECT Qty FROM T) d", 3L },
            { "SELECT d.q + 1 FROM (SELECT Qty AS q FROM T) d", 3L },
            { "SELECT d.q + 'x' FROM (SELECT Name AS q FROM T) d", "abx" },
            { "SELECT d.v + 'x' FROM (SELECT Name + 'y' AS v FROM T) d", "abyx" },
            { "SELECT x.Name + y.Name FROM T x JOIN T y ON x.Qty = y.Qty", "abab" },
            { "SELECT (SELECT Name FROM T) + 'x'", "abx" },
        };

    [Theory]
    [MemberData(nameof(LiteralCases))]
    public void Plus_OnLiterals_BehavesLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(ColumnCases))]
    public void Plus_OnTableColumns_UsesTheDeclaredColumnTypes(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(RelationCases))]
    public void Plus_OverCtesAndDerivedTables_FollowsTheInferredColumnTypes(
        string sql,
        object? expected
    )
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Fact]
    public void Plus_InUpdateSet_ConcatenatesTextColumns()
    {
        _database.RunSuccessfully("UPDATE T SET Name = Name + 'z'");

        Assert.Equal("abz", _database.Scalar("SELECT Name FROM T"));
    }

    [Fact]
    public void Plus_InUpdateSet_AddsNumericColumns()
    {
        _database.RunSuccessfully("UPDATE T SET Qty = Qty + 10");

        Assert.Equal(12L, _database.Scalar("SELECT Qty FROM T"));
    }

    [Fact]
    public void Plus_InUpdateSetWithSeveralAssignments_TypesEachAssignmentOnItsOwn()
    {
        _database.RunSuccessfully("UPDATE T SET Name = Name + 'q', Qty = Qty + 1");

        ResultSet row = _database.Query("SELECT Name, Qty FROM T");
        Assert.Equal("abq", row.Rows[0][0]);
        Assert.Equal(3L, row.Rows[0][1]);
    }

    [Fact]
    public void Plus_InInsertValues_ConcatenatesLiterals()
    {
        _database.RunSuccessfully("INSERT INTO T (Name, Qty) VALUES ('a' + 'b', 1 + 1)");

        Assert.Equal("ab", _database.Scalar("SELECT Name FROM T WHERE Qty = 2 AND Stamp IS NULL"));
    }

    [Fact]
    public void Plus_InWhereClause_ConcatenatesBeforeComparing()
    {
        Assert.Equal(1L, _database.Scalar("SELECT COUNT(*) FROM T WHERE Name + 'x' = 'abx'"));
    }

    [Fact]
    public void Plus_InOrderBy_ConcatenatesBeforeSorting()
    {
        _database.RunSuccessfully("INSERT INTO T (Name, Qty) VALUES ('b', 1)");

        ResultSet result = _database.Query("SELECT Name FROM T ORDER BY Name + 'x' DESC");

        Assert.Equal(["b", "ab"], result.Column());
    }

    [Fact]
    public void Plus_InGroupByExpression_Concatenates()
    {
        Assert.Equal("abx", _database.Scalar("SELECT Name + 'x' FROM T GROUP BY Name + 'x'"));
    }

    [Fact]
    public void Plus_OnDatetimeColumnAndNumber_AddsDays()
    {
        Assert.Equal("2024-02-01 10:00:00.000", _database.Scalar("SELECT Stamp + 1 FROM T"));
    }

    [Fact]
    public void Plus_OnNumberAndDatetimeColumn_AddsDays()
    {
        Assert.Equal("2024-02-02 10:00:00.000", _database.Scalar("SELECT 2 + Stamp FROM T"));
    }

    [Fact]
    public void Plus_OnDatetimeColumnAndFraction_AddsPartOfADay()
    {
        Assert.Equal("2024-01-31 22:00:00.000", _database.Scalar("SELECT Stamp + 0.5 FROM T"));
    }

    [Fact]
    public void Plus_OnGetdateAndNumber_AddsDaysToTheFixedClock()
    {
        Assert.Equal("2024-03-16 10:20:30.123", _database.Scalar("SELECT GETDATE() + 1"));
    }

    [Fact]
    public void Minus_OnDatetimeColumnAndNumber_SubtractsDays()
    {
        Assert.Equal("2024-01-30 10:00:00.000", _database.Scalar("SELECT Stamp - 1 FROM T"));
    }

    [Fact]
    public void Plus_OnCastDatetimeAndNumber_AddsDays()
    {
        Assert.Equal(
            "2024-02-01 00:00:00.000",
            _database.Scalar("SELECT CAST('2024-01-31' AS DATETIME) + 1 AS d")
        );
    }

    [Fact]
    public void Plus_OnTextCompoundAssignmentInSet_UsesConcatenation()
    {
        Assert.Equal(
            "ab",
            _database.Scalar("DECLARE @s VARCHAR(10) = 'a'; SET @s += 'b'; SELECT @s")
        );
    }

    [Fact]
    public void Plus_OnNumericCompoundAssignmentInSet_Adds()
    {
        Assert.Equal(5L, _database.Scalar("DECLARE @i INT = 1; SET @i += 4; SELECT @i"));
    }

    [Fact]
    public void Plus_OnVariables_UsesTheDeclaredTypes()
    {
        Assert.Equal(
            "ab",
            _database.Scalar(
                "DECLARE @a VARCHAR(5) = 'a'; DECLARE @b VARCHAR(5) = 'b'; SELECT @a + @b"
            )
        );
        Assert.Equal(
            3L,
            _database.Scalar("DECLARE @a INT = 1; DECLARE @b INT = 2; SELECT @a + @b")
        );
    }

    [Fact]
    public void Plus_OnTextVariableAndNumericLiteralText_ConcatenatesOnlyWhenBothAreText()
    {
        Assert.Equal("a1", _database.Scalar("DECLARE @a VARCHAR(5) = 'a'; SELECT @a + '1'"));
    }
}

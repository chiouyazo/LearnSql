using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class StringAggTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public StringAggTests()
    {
        _database.ExecuteRaw(
            "CREATE TABLE G (Grp INT, Name VARCHAR(5) NULL, Rank INT);"
                + "INSERT INTO G VALUES (1, 'b', 2), (1, 'a', 1), (1, NULL, 3), (2, 'z', 1), (2, 'y', 2);"
        );
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    [Fact]
    public void StringAgg_WithinGroupAscending_OrdersTheItems()
    {
        Assert.Equal(
            "a,b",
            _database.Scalar(
                "SELECT STRING_AGG(Name, ',') WITHIN GROUP (ORDER BY Name) FROM G WHERE Grp = 1"
            )
        );
    }

    [Fact]
    public void StringAgg_WithinGroupDescending_OrdersTheItemsBackwards()
    {
        Assert.Equal(
            "b,a",
            _database.Scalar(
                "SELECT STRING_AGG(Name, ',') WITHIN GROUP (ORDER BY Name DESC) FROM G WHERE Grp = 1"
            )
        );
    }

    [Fact]
    public void StringAgg_WithinGroupOrderedByAnotherColumn_UsesThatColumn()
    {
        Assert.Equal(
            "z-y",
            _database.Scalar(
                "SELECT STRING_AGG(Name, '-') WITHIN GROUP (ORDER BY Rank) FROM G WHERE Grp = 2"
            )
        );
    }

    [Fact]
    public void StringAgg_IgnoresNullValues()
    {
        string text = (string)
            _database.Scalar(
                "SELECT STRING_AGG(Name, ',') WITHIN GROUP (ORDER BY Rank) FROM G WHERE Grp = 1"
            )!;

        Assert.Equal("a,b", text);
    }

    [Fact]
    public void StringAgg_WithGroupBy_AggregatesEachGroup()
    {
        ResultSet result = _database.Query(
            "SELECT Grp, STRING_AGG(Name, ', ') WITHIN GROUP (ORDER BY Name) FROM G GROUP BY Grp ORDER BY Grp"
        );

        Assert.Equal(["a, b", "y, z"], result.Column(1));
    }

    [Fact]
    public void StringAgg_OverNoRows_ReturnsNull()
    {
        Assert.Null(_database.Scalar("SELECT STRING_AGG(Name, ',') FROM G WHERE Grp = 99"));
    }

    [Fact]
    public void StringAgg_OverConvertedNumbers_JoinsTheirText()
    {
        Assert.Equal(
            "1-2-3",
            _database.Scalar(
                "SELECT STRING_AGG(CAST(Rank AS VARCHAR(5)), '-') WITHIN GROUP (ORDER BY Rank) FROM G WHERE Grp = 1"
            )
        );
    }

    [Fact]
    public void StringAgg_WithoutWithinGroup_StillAggregatesAllItems()
    {
        string text = (string)
            _database.Scalar("SELECT STRING_AGG(Name, ',') FROM G WHERE Grp = 2")!;

        Assert.Equal(["y", "z"], text.Split(',').Order());
    }

    [Fact]
    public void StringAgg_WithinGroup_IsRewrittenToTheOrderByInsideTheCall()
    {
        string translated = _database
            .Translate("SELECT STRING_AGG(Name, ',') WITHIN GROUP (ORDER BY Name) FROM G")
            .Batches[0]
            .Statements[0]
            .Sql;

        Assert.Equal("SELECT STRING_AGG(Name, ',' ORDER BY Name) FROM G", translated);
    }
}

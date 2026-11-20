using LearnSql.SqlFormat;

namespace LearnSql.Tests.Formatting;

public class SqlFormatterTests
{
    private const int MaxInlineLength = 60;

    public static TheoryData<string, string, string> Cases()
    {
        TheoryData<string, string, string> data = new TheoryData<string, string, string>();
        data.Add(
            "StarSelectStaysOnOneLine",
            "select * from Customer",
            Sql("SELECT * FROM Customer")
        );
        data.Add(
            "ManyConditionsBreakIntoClauses",
            "select a, b, c from T where x = 1 and y = 2 and z = 3 order by a",
            Sql(
                "SELECT",
                "    a,",
                "    b,",
                "    c",
                "FROM T",
                "WHERE x = 1",
                "    AND y = 2",
                "    AND z = 3",
                "ORDER BY a"
            )
        );
        data.Add(
            "ShortDistinctStaysOnOneLine",
            "select distinct City, Country from Customer order by City",
            Sql("SELECT DISTINCT City, Country FROM Customer ORDER BY City")
        );
        data.Add(
            "LeftOuterJoinWithExtraOnCondition",
            "SELECT c.Name, o.Id FROM Customer c LEFT OUTER JOIN Orders o ON o.CustomerId = c.Id AND o.Status <> 'Cancelled' WHERE c.Active = 1 OR c.Vip = 1",
            Sql(
                "SELECT",
                "    c.Name,",
                "    o.Id",
                "FROM Customer c",
                "LEFT OUTER JOIN Orders o ON o.CustomerId = c.Id",
                "    AND o.Status <> 'Cancelled'",
                "WHERE c.Active = 1",
                "    OR c.Vip = 1"
            )
        );
        data.Add(
            "BetweenAndIsNotSplit",
            "SELECT Name, Price FROM Product WHERE Price BETWEEN 5 AND 10 AND Category = 'Kaffee' AND Stock > 0",
            Sql(
                "SELECT",
                "    Name,",
                "    Price",
                "FROM Product",
                "WHERE Price BETWEEN 5 AND 10",
                "    AND Category = 'Kaffee'",
                "    AND Stock > 0"
            )
        );
        data.Add(
            "OffsetFetchOnOwnLines",
            "SELECT Id, Name FROM Product ORDER BY Name OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY",
            Sql(
                "SELECT",
                "    Id,",
                "    Name",
                "FROM Product",
                "ORDER BY Name",
                "OFFSET 10 ROWS",
                "FETCH NEXT 5 ROWS ONLY"
            )
        );
        data.Add(
            "UnionAllAndUnion",
            "select a, b from T1 union all select a, b from T2 union select a, b from T3 order by a",
            Sql(
                "SELECT",
                "    a,",
                "    b",
                "FROM T1",
                "UNION ALL",
                "SELECT",
                "    a,",
                "    b",
                "FROM T2",
                "UNION",
                "SELECT",
                "    a,",
                "    b",
                "FROM T3",
                "ORDER BY a"
            )
        );
        data.Add(
            "ExistsAndInSubqueries",
            "SELECT Name FROM Customer c WHERE EXISTS (SELECT 1 FROM Orders o WHERE o.CustomerId = c.Id) AND Id NOT IN (SELECT CustomerId FROM Blocked)",
            Sql(
                "SELECT Name",
                "FROM Customer c",
                "WHERE EXISTS (",
                "    SELECT 1",
                "    FROM Orders o",
                "    WHERE o.CustomerId = c.Id",
                ")",
                "    AND Id NOT IN (",
                "        SELECT CustomerId",
                "        FROM Blocked",
                "    )"
            )
        );
        data.Add(
            "TwoCommonTableExpressions",
            "WITH Totals AS (SELECT CustomerId, SUM(Amount) AS Total FROM Payment GROUP BY CustomerId), Ranked AS (SELECT CustomerId, Total, RANK() OVER (ORDER BY Total DESC) AS Rnk FROM Totals) SELECT c.Name, r.Total FROM Ranked r INNER JOIN Customer c ON c.Id = r.CustomerId WHERE r.Rnk <= 3",
            Sql(
                "WITH Totals AS (",
                "    SELECT",
                "        CustomerId,",
                "        SUM(Amount) AS Total",
                "    FROM Payment",
                "    GROUP BY CustomerId",
                "),",
                "Ranked AS (",
                "    SELECT",
                "        CustomerId,",
                "        Total,",
                "        RANK() OVER (ORDER BY Total DESC) AS Rnk",
                "    FROM Totals",
                ")",
                "SELECT",
                "    c.Name,",
                "    r.Total",
                "FROM Ranked r",
                "INNER JOIN Customer c ON c.Id = r.CustomerId",
                "WHERE r.Rnk <= 3"
            )
        );
        data.Add(
            "SearchedCase",
            "SELECT Id, CASE WHEN Price < 5 THEN 'cheap' WHEN Price < 20 THEN 'normal' ELSE 'expensive' END AS Tier, Name FROM Product",
            Sql(
                "SELECT",
                "    Id,",
                "    CASE",
                "        WHEN Price < 5 THEN 'cheap'",
                "        WHEN Price < 20 THEN 'normal'",
                "        ELSE 'expensive'",
                "    END AS Tier,",
                "    Name",
                "FROM Product"
            )
        );
        data.Add(
            "SimpleCase",
            "SELECT CASE Status WHEN 'Open' THEN 1 ELSE 0 END FROM Orders",
            Sql(
                "SELECT",
                "    CASE Status",
                "        WHEN 'Open' THEN 1",
                "        ELSE 0",
                "    END",
                "FROM Orders"
            )
        );
        data.Add(
            "NestedCase",
            "SELECT Id, CASE WHEN a > 1 THEN CASE WHEN b > 1 THEN 'x' ELSE 'y' END ELSE 'z' END AS Nested FROM T",
            Sql(
                "SELECT",
                "    Id,",
                "    CASE",
                "        WHEN a > 1 THEN CASE",
                "            WHEN b > 1 THEN 'x'",
                "            ELSE 'y'",
                "        END",
                "        ELSE 'z'",
                "    END AS Nested",
                "FROM T"
            )
        );
        data.Add(
            "WindowFunctionsPartitionAndOrder",
            "SELECT Name, ROW_NUMBER() OVER (PARTITION BY Country ORDER BY Name) AS Rn, SUM(x) OVER (PARTITION BY Country) AS S FROM Customer",
            Sql(
                "SELECT",
                "    Name,",
                "    ROW_NUMBER() OVER (",
                "        PARTITION BY Country",
                "        ORDER BY Name",
                "    ) AS Rn,",
                "    SUM(x) OVER (PARTITION BY Country) AS S",
                "FROM Customer"
            )
        );
        data.Add(
            "InsertSelect",
            "INSERT INTO Review (Id, CustomerId) SELECT Id, 1 FROM Customer WHERE City = 'Hamburg' AND Email IS NOT NULL",
            Sql(
                "INSERT INTO Review (Id, CustomerId)",
                "SELECT",
                "    Id,",
                "    1",
                "FROM Customer",
                "WHERE City = 'Hamburg'",
                "    AND Email IS NOT NULL"
            )
        );
        data.Add(
            "UpdateWithSeveralAssignments",
            "UPDATE Product SET Price = Price * 1.1, Stock = 0 WHERE Category = 'Kaffee' AND Stock IS NULL",
            Sql(
                "UPDATE Product",
                "SET",
                "    Price = Price * 1.1,",
                "    Stock = 0",
                "WHERE Category = 'Kaffee'",
                "    AND Stock IS NULL"
            )
        );
        data.Add(
            "UpdateFromJoin",
            "UPDATE p SET Price = o.NewPrice FROM Product p INNER JOIN PriceUpdate o ON o.ProductId = p.Id WHERE o.Valid = 1",
            Sql(
                "UPDATE p",
                "SET Price = o.NewPrice",
                "FROM Product p",
                "INNER JOIN PriceUpdate o ON o.ProductId = p.Id",
                "WHERE o.Valid = 1"
            )
        );
        data.Add(
            "DeleteWithLongCondition",
            "DELETE FROM Orders WHERE Status = 'Cancelled' AND OrderDate < '2020-01-01' AND ShippedOn IS NULL",
            Sql(
                "DELETE FROM Orders",
                "WHERE Status = 'Cancelled'",
                "    AND OrderDate < '2020-01-01'",
                "    AND ShippedOn IS NULL"
            )
        );
        data.Add(
            "DeleteWithJoin",
            "DELETE o FROM Orders o INNER JOIN Customer c ON c.Id = o.CustomerId WHERE c.Active = 0",
            Sql(
                "DELETE o",
                "FROM Orders o",
                "INNER JOIN Customer c ON c.Id = o.CustomerId",
                "WHERE c.Active = 0"
            )
        );
        data.Add(
            "ShortCreateView",
            "create view V as select a from T",
            Sql("CREATE VIEW V AS SELECT a FROM T")
        );
        data.Add(
            "CreateTableDefinitions",
            "create table T (id int not null primary key, name nvarchar(max) null, price decimal(10, 2) default 0, created datetime2 not null default getdate())",
            Sql(
                "CREATE TABLE T (",
                "    id INT NOT NULL PRIMARY KEY,",
                "    name NVARCHAR(MAX) NULL,",
                "    price DECIMAL(10, 2) DEFAULT 0,",
                "    created DATETIME2 NOT NULL DEFAULT GETDATE()",
                ")"
            )
        );
        data.Add(
            "OperatorSpacingAndLiterals",
            "SELECT a<>b, c!=d, x>=1, -1, y*-2, - z, N'it''s', [Order].*, COUNT(*), @v, #t.x FROM [Order]",
            Sql(
                "SELECT",
                "    a <> b,",
                "    c != d,",
                "    x >= 1,",
                "    -1,",
                "    y * -2,",
                "    -z,",
                "    N'it''s',",
                "    [Order].*,",
                "    COUNT(*),",
                "    @v,",
                "    #t.x",
                "FROM [Order]"
            )
        );
        data.Add(
            "CommaJoinedTables",
            "SELECT a FROM x, y WHERE x.id = y.id AND y.name = 'abcdefghijklmnop' ORDER BY a",
            Sql(
                "SELECT a",
                "FROM x,",
                "    y",
                "WHERE x.id = y.id",
                "    AND y.name = 'abcdefghijklmnop'",
                "ORDER BY a"
            )
        );
        data.Add(
            "BatchSeparators",
            Sql("SELECT 1 AS a;", "GO", "SELECT 2 AS b", "GO 3"),
            Sql("SELECT 1 AS a;", "GO", "", "SELECT 2 AS b", "GO 3")
        );
        data.Add(
            "SelectInto",
            "SELECT Id, Price INTO #Copy FROM Product WHERE Price > 5 AND Category = 'Kaffee' AND Stock > 0",
            Sql(
                "SELECT",
                "    Id,",
                "    Price",
                "INTO #Copy",
                "FROM Product",
                "WHERE Price > 5",
                "    AND Category = 'Kaffee'",
                "    AND Stock > 0"
            )
        );
        data.Add(
            "FunctionAndTypeCasing",
            "SELECT TOP 5 PERCENT cast(Price as int) AS p, convert(varchar(10), OrderDate, 120) AS d, month(OrderDate) AS m, ISNULL(x, 0) FROM Product ORDER BY Price",
            Sql(
                "SELECT TOP 5 PERCENT",
                "    CAST(Price AS INT) AS p,",
                "    CONVERT(VARCHAR(10), OrderDate, 120) AS d,",
                "    MONTH(OrderDate) AS m,",
                "    ISNULL(x, 0)",
                "FROM Product",
                "ORDER BY Price"
            )
        );
        data.Add(
            "TopWithTies",
            "SELECT TOP (10) WITH TIES Name, Price FROM Product ORDER BY Price DESC",
            Sql(
                "SELECT TOP (10) WITH TIES",
                "    Name,",
                "    Price",
                "FROM Product",
                "ORDER BY Price DESC"
            )
        );
        data.Add(
            "CrossAndOuterApply",
            "SELECT a, b FROM T CROSS APPLY fn(a) f OUTER APPLY (SELECT TOP 1 x FROM U WHERE U.a = T.a ORDER BY x) u WHERE a = 1",
            Sql(
                "SELECT",
                "    a,",
                "    b",
                "FROM T",
                "CROSS APPLY fn(a) f",
                "OUTER APPLY (",
                "    SELECT TOP 1 x",
                "    FROM U",
                "    WHERE U.a = T.a",
                "    ORDER BY x",
                ") u",
                "WHERE a = 1"
            )
        );
        data.Add(
            "DeclareStatement",
            "DECLARE @x int = 5; SELECT @x",
            Sql("DECLARE @x INT = 5;", "", "SELECT @x")
        );
        data.Add(
            "GroupByAndHaving",
            "SELECT Name, COUNT(*) AS Orders FROM Customer c INNER JOIN Orders o ON o.CustomerId = c.Id GROUP BY Name HAVING COUNT(*) > 1 AND SUM(o.Total) > 100 ORDER BY Orders DESC, Name",
            Sql(
                "SELECT",
                "    Name,",
                "    COUNT(*) AS Orders",
                "FROM Customer c",
                "INNER JOIN Orders o ON o.CustomerId = c.Id",
                "GROUP BY Name",
                "HAVING COUNT(*) > 1",
                "    AND SUM(o.Total) > 100",
                "ORDER BY Orders DESC, Name"
            )
        );
        data.Add(
            "ScalarSubqueries",
            "SELECT (SELECT MAX(x) FROM T) AS m, (SELECT 1) AS one, Name FROM Customer WHERE Id IN (1, 2, 3) AND (a = 1 OR b = 2)",
            Sql(
                "SELECT",
                "    (",
                "        SELECT MAX(x)",
                "        FROM T",
                "    ) AS m,",
                "    (SELECT 1) AS one,",
                "    Name",
                "FROM Customer",
                "WHERE Id IN (1, 2, 3)",
                "    AND (a = 1 OR b = 2)"
            )
        );
        return data;
    }

    private static string Sql(params string[] lines)
    {
        return string.Join("\n", lines);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_ProducesTheExpectedLayout(string name, string input, string expected)
    {
        Assert.Equal(expected, SqlFormatter.Format(input));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_IsIdempotent(string name, string input, string expected)
    {
        string once = SqlFormatter.Format(input);

        Assert.Equal(once, SqlFormatter.Format(once));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_LeavesExpectedOutputUnchanged(string name, string input, string expected)
    {
        Assert.Equal(expected, SqlFormatter.Format(expected));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_UsesOnlyLineFeedsSpacesAndNoTrailingBlanks(
        string name,
        string input,
        string expected
    )
    {
        string formatted = SqlFormatter.Format(input);

        Assert.DoesNotContain('\t', formatted);
        Assert.DoesNotContain('\r', formatted);
        Assert.All(formatted.Split('\n'), (string line) => Assert.Equal(line.TrimEnd(), line));
    }

    [Fact]
    public void Format_StatementOfExactlyTheLimitStaysOnOneLine()
    {
        string sql = "SELECT a, b FROM T WHERE x = 1 AND y = 222222222222222222222";

        Assert.Equal(MaxInlineLength, sql.Length);
        Assert.Equal(sql, SqlFormatter.Format(sql));
    }

    [Fact]
    public void Format_StatementOneCharacterOverTheLimitIsBroken()
    {
        string sql = "SELECT a, b FROM T WHERE x = 1 AND y = 2222222222222222222222";

        string formatted = SqlFormatter.Format(sql);

        Assert.Equal(MaxInlineLength + 1, sql.Length);
        Assert.Equal(
            Sql(
                "SELECT",
                "    a,",
                "    b",
                "FROM T",
                "WHERE x = 1",
                "    AND y = 2222222222222222222222"
            ),
            formatted
        );
    }

    [Fact]
    public void Format_TrailingSemicolonDoesNotCountTowardsTheLimit()
    {
        string sql = "SELECT a, b FROM T WHERE x = 1 AND y = 222222222222222222222;";

        Assert.Equal(sql, SqlFormatter.Format(sql));
    }

    [Fact]
    public void Format_KeepsStringLiteralsAndQuotedIdentifiersUntouched()
    {
        string formatted = SqlFormatter.Format(
            "select '  select  from  ', [My Col] from [My Table]"
        );

        Assert.Equal("SELECT '  select  from  ', [My Col] FROM [My Table]", formatted);
    }

    [Fact]
    public void Format_KeepsIdentifierCase()
    {
        Assert.Equal(
            "SELECT firstName FROM dbo.Customer c",
            SqlFormatter.Format("select firstName from dbo.Customer c")
        );
    }

    [Fact]
    public void Format_ColumnNamedLikeAContextWordIsNotUppercased()
    {
        Assert.Equal(
            "SELECT offset, rows FROM T",
            SqlFormatter.Format("select offset, rows from T")
        );
    }

    [Fact]
    public void Format_EmptyAndBlankInputGiveAnEmptyString()
    {
        Assert.Equal(string.Empty, SqlFormatter.Format(string.Empty));
        Assert.Equal(string.Empty, SqlFormatter.Format("  \n\t "));
    }

    [Fact]
    public void Format_NormalisesLineEndingsAndIndentationOfTheInput()
    {
        string messy =
            "SELECT\r\n\ta,\r\n\t\t b\r\n  FROM   T\r\n WHERE x=1 AND y=2 AND z=3 AND w=4 AND v=5";

        string formatted = SqlFormatter.Format(messy);

        Assert.Equal(
            Sql(
                "SELECT",
                "    a,",
                "    b",
                "FROM T",
                "WHERE x = 1",
                "    AND y = 2",
                "    AND z = 3",
                "    AND w = 4",
                "    AND v = 5"
            ),
            formatted
        );
    }

    [Fact]
    public void Format_ThrowsForComments()
    {
        Assert.Throws<FormatException>(() => SqlFormatter.Format("SELECT 1 -- note"));
        Assert.Throws<FormatException>(() => SqlFormatter.Format("SELECT /* note */ 1"));
    }

    [Fact]
    public void Format_ThrowsForUnterminatedLiterals()
    {
        Assert.Throws<FormatException>(() => SqlFormatter.Format("SELECT 'abc"));
    }

    [Fact]
    public void Format_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() => SqlFormatter.Format(null!));
    }
}

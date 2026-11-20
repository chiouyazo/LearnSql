using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public class InformationSchemaViewTests : IDisposable
{
    private readonly SqliteTestDatabase _database = SqliteTestDatabase.ForScenario("starter");

    public void Dispose()
    {
        _database.Dispose();
    }

    private IReadOnlyList<object?> Row(string sql)
    {
        ResultSet result = _database.Query(sql);
        Assert.Single(result.Rows);
        return result.Rows[0];
    }

    [Fact]
    public void Tables_HaveTheSqlServerColumnNamesInOrder()
    {
        ResultSet result = _database.Query("SELECT * FROM TABLES");

        Assert.Equal(["TABLE_CATALOG", "TABLE_SCHEMA", "TABLE_NAME", "TABLE_TYPE"], result.Columns);
    }

    [Fact]
    public void Tables_AreAlwaysInSchemaDboAndCatalogLearnSql()
    {
        ResultSet result = _database.Query("SELECT TABLE_CATALOG, TABLE_SCHEMA FROM TABLES");

        Assert.NotEmpty(result.Rows);
        Assert.All(
            result.Rows,
            (IReadOnlyList<object?> row) =>
            {
                Assert.Equal("LearnSql", row[0]);
                Assert.Equal("dbo", row[1]);
            }
        );
    }

    [Fact]
    public void Tables_AreAvailableWithTheInformationSchemaPrefix()
    {
        Assert.Equal(
            _database.Scalar("SELECT COUNT(*) FROM TABLES"),
            _database.Scalar("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES")
        );
    }

    [Fact]
    public void Tables_ListTheUserTablesOnly()
    {
        string[] expected = ["Customer", "OrderItem", "Orders", "Product"];

        Assert.Equal(
            expected,
            _database.Query("SELECT TABLE_NAME FROM TABLES").Column().Cast<string>().Order()
        );
    }

    [Fact]
    public void Tables_ExcludeInternalSqliteTablesAndTheViewsThemselves()
    {
        Assert.Equal(
            1L,
            _database.Scalar("SELECT COUNT(*) FROM sqlite_master WHERE name = 'sqlite_sequence'")
        );

        ResultSet names = _database.Query("SELECT TABLE_NAME FROM TABLES");

        Assert.DoesNotContain(
            names.Column(),
            (object? name) => ((string)name!).StartsWith("sqlite_")
        );
        Assert.DoesNotContain("TABLES", names.Column());
        Assert.DoesNotContain("COLUMNS", names.Column());
    }

    [Fact]
    public void Tables_IncludeUserTablesWhoseNameOnlyLooksLikeTheSqlitePrefix()
    {
        _database.ExecuteRaw("CREATE TABLE sqliteXdata (a INT)");

        Assert.Equal(
            1L,
            _database.Scalar("SELECT COUNT(*) FROM TABLES WHERE TABLE_NAME = 'sqliteXdata'")
        );
    }

    [Fact]
    public void Tables_ReportBaseTablesAsBaseTable()
    {
        ResultSet types = _database.Query("SELECT DISTINCT TABLE_TYPE FROM TABLES");

        Assert.Equal("BASE TABLE", Assert.Single(types.Rows)[0]);
    }

    [Fact]
    public void Tables_ReportUserViewsWithTypeView()
    {
        _database.ExecuteRaw("CREATE VIEW CustomerCities AS SELECT City FROM Customer");

        IReadOnlyList<object?> row = Row(
            "SELECT TABLE_NAME, TABLE_TYPE FROM TABLES WHERE TABLE_NAME = 'CustomerCities'"
        );

        Assert.Equal(["CustomerCities", "VIEW"], row);
    }

    [Fact]
    public void Tables_TemplateQueryWithBaseTableInMixedCase_ReturnsAllBaseTables()
    {
        ResultSet result = _database.Query("SELECT * FROM TABLES WHERE TABLE_TYPE='Base Table'");

        Assert.Equal(4, result.RowCount);
    }

    [Fact]
    public void Tables_NameComparisonIgnoresCase()
    {
        Assert.Equal(
            1L,
            _database.Scalar("SELECT COUNT(*) FROM TABLES WHERE TABLE_NAME = 'customer'")
        );
    }

    [Fact]
    public void Columns_HaveTheSqlServerColumnNamesInOrder()
    {
        ResultSet result = _database.Query("SELECT * FROM COLUMNS");

        Assert.Equal(
            [
                "TABLE_CATALOG",
                "TABLE_SCHEMA",
                "TABLE_NAME",
                "COLUMN_NAME",
                "ORDINAL_POSITION",
                "COLUMN_DEFAULT",
                "IS_NULLABLE",
                "DATA_TYPE",
                "CHARACTER_MAXIMUM_LENGTH",
                "NUMERIC_PRECISION",
                "NUMERIC_SCALE",
                "DATETIME_PRECISION",
            ],
            result.Columns
        );
    }

    [Fact]
    public void Columns_OrdinalPositionIsOneBasedAndFollowsTheDeclarationOrder()
    {
        ResultSet result = _database.Query(
            "SELECT COLUMN_NAME, ORDINAL_POSITION FROM COLUMNS WHERE TABLE_NAME = 'Customer' ORDER BY ORDINAL_POSITION"
        );

        Assert.Equal(
            ["Id", "FirstName", "LastName", "City", "Email", "RegisteredOn"],
            result.Column()
        );
        Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L], result.Column(1));
    }

    [Fact]
    public void Columns_AreInSchemaDboAndNeverListTheSystemViews()
    {
        Assert.Equal(
            0L,
            _database.Scalar("SELECT COUNT(*) FROM COLUMNS WHERE TABLE_SCHEMA <> 'dbo'")
        );
        Assert.Equal(
            0L,
            _database.Scalar(
                "SELECT COUNT(*) FROM COLUMNS WHERE TABLE_NAME IN ('TABLES', 'COLUMNS') OR TABLE_NAME LIKE 'sqlite%'"
            )
        );
    }

    [Theory]
    [InlineData("Id", "NO")]
    [InlineData("FirstName", "NO")]
    [InlineData("City", "YES")]
    [InlineData("Email", "YES")]
    [InlineData("RegisteredOn", "NO")]
    public void Columns_IsNullableReflectsNotNullAndPrimaryKey(string column, string expected)
    {
        Assert.Equal(
            expected,
            _database.Scalar(
                $"SELECT IS_NULLABLE FROM COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = '{column}'"
            )
        );
    }

    [Theory]
    [InlineData("Customer", "Id", "int", null, 10L, 0L, null)]
    [InlineData("Customer", "FirstName", "nvarchar", 50L, null, null, null)]
    [InlineData("Customer", "Email", "nvarchar", 100L, null, null, null)]
    [InlineData("Customer", "RegisteredOn", "date", null, null, null, 0L)]
    [InlineData("Product", "Price", "decimal", null, 8L, 2L, null)]
    [InlineData("Product", "Stock", "int", null, 10L, 0L, null)]
    public void Columns_UseSqlServerTypeNamesWithLengthPrecisionAndScale(
        string table,
        string column,
        string dataType,
        long? length,
        long? precision,
        long? scale,
        long? dateTimePrecision
    )
    {
        IReadOnlyList<object?> row = Row(
            "SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION "
                + $"FROM COLUMNS WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}'"
        );

        Assert.Equal(dataType, row[0]);
        Assert.Equal(length, row[1]);
        Assert.Equal(precision, row[2]);
        Assert.Equal(scale, row[3]);
        Assert.Equal(dateTimePrecision, row[4]);
    }

    [Fact]
    public void Columns_MapTheCommonDeclaredTypesToSqlServerTypeInformation()
    {
        _database.RunSuccessfully(
            "CREATE TABLE TypeProbe ("
                + "A BIGINT, B SMALLINT, C TINYINT, D BIT, E FLOAT, F DECIMAL(10,3), G NUMERIC(6), "
                + "H CHAR(3), I NCHAR(2), J VARCHAR(20), L DATETIME2(3), M TIME(5), "
                + "N DATETIME, O DATE, P VARBINARY(16), Q INTEGER, R DECIMAL, S NVARCHAR, T DATETIME2, U INT DEFAULT 5)"
        );

        ResultSet result = _database.Query(
            "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION "
                + "FROM COLUMNS WHERE TABLE_NAME = 'TypeProbe' ORDER BY ORDINAL_POSITION"
        );

        Dictionary<string, IReadOnlyList<object?>> byColumn = result.Rows.ToDictionary(
            (IReadOnlyList<object?> row) => (string)row[0]!
        );
        Assert.Equal(["bigint", null, 19L, 0L, null], byColumn["A"].Skip(1));
        Assert.Equal(["smallint", null, 5L, 0L, null], byColumn["B"].Skip(1));
        Assert.Equal(["tinyint", null, 3L, 0L, null], byColumn["C"].Skip(1));
        Assert.Equal(["bit", null, 1L, 0L, null], byColumn["D"].Skip(1));
        Assert.Equal(["float", null, 53L, null, null], byColumn["E"].Skip(1));
        Assert.Equal(["decimal", null, 10L, 3L, null], byColumn["F"].Skip(1));
        Assert.Equal(["numeric", null, 6L, 0L, null], byColumn["G"].Skip(1));
        Assert.Equal(["char", 3L, null, null, null], byColumn["H"].Skip(1));
        Assert.Equal(["nchar", 2L, null, null, null], byColumn["I"].Skip(1));
        Assert.Equal(["varchar", 20L, null, null, null], byColumn["J"].Skip(1));
        Assert.Equal(["datetime2", null, null, null, 3L], byColumn["L"].Skip(1));
        Assert.Equal(["time", null, null, null, 5L], byColumn["M"].Skip(1));
        Assert.Equal(["datetime", null, null, null, 3L], byColumn["N"].Skip(1));
        Assert.Equal(["date", null, null, null, 0L], byColumn["O"].Skip(1));
        Assert.Equal(["varbinary", 16L, null, null, null], byColumn["P"].Skip(1));
        Assert.Equal(["int", null, 10L, 0L, null], byColumn["Q"].Skip(1));
        Assert.Equal(["decimal", null, 18L, 0L, null], byColumn["R"].Skip(1));
        Assert.Equal(["nvarchar", 1L, null, null, null], byColumn["S"].Skip(1));
        Assert.Equal(["datetime2", null, null, null, 7L], byColumn["T"].Skip(1));
    }

    [Theory]
    [InlineData("NVARCHAR(MAX)", "nvarchar")]
    [InlineData("VARCHAR(MAX)", "varchar")]
    [InlineData("VARBINARY(MAX)", "varbinary")]
    [InlineData("nvarchar ( max )", "nvarchar")]
    [InlineData("NVARCHAR(max)", "nvarchar")]
    public void Columns_OfMaxTypes_ReportLengthMinusOne(string declaredType, string dataType)
    {
        _database.RunSuccessfully($"CREATE TABLE MaxProbe (Body {declaredType})");

        IReadOnlyList<object?> row = Row(
            "SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM COLUMNS WHERE TABLE_NAME = 'MaxProbe'"
        );

        Assert.Equal([dataType, -1L], row);
    }

    [Fact]
    public void Columns_OfMaxTypes_DoNotAffectTheNeighbouringColumns()
    {
        _database.RunSuccessfully(
            "CREATE TABLE MaxMix (A NVARCHAR(MAX) NOT NULL, B NVARCHAR(40), C VARCHAR(MAX) NULL, D INT)"
        );

        ResultSet result = _database.Query(
            "SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM COLUMNS WHERE TABLE_NAME = 'MaxMix' ORDER BY ORDINAL_POSITION"
        );

        Assert.Equal([-1L, 40L, -1L, null], result.Column(1));
        Assert.Equal(["NO", "YES", "YES", "YES"], result.Column(2));
    }

    [Fact]
    public void Columns_OfAnAddedMaxColumn_ReportLengthMinusOne()
    {
        _database.RunSuccessfully("CREATE TABLE MaxLate (Id INT)");
        _database.RunSuccessfully("ALTER TABLE MaxLate ADD Notes NVARCHAR(MAX)");

        object? length = _database.Scalar(
            "SELECT CHARACTER_MAXIMUM_LENGTH FROM COLUMNS WHERE TABLE_NAME = 'MaxLate' AND COLUMN_NAME = 'Notes'"
        );

        Assert.Equal(-1L, length);
    }

    [Fact]
    public void MaxColumns_StoreLongTextWithoutTruncation()
    {
        string text = new string('x', 5000);
        _database.RunSuccessfully("CREATE TABLE MaxStore (Body NVARCHAR(MAX))");
        _database.RunSuccessfully($"INSERT INTO MaxStore VALUES ('{text}')");

        Assert.Equal(text, _database.Scalar("SELECT Body FROM MaxStore"));
    }

    [Fact]
    public void Columns_ReportTheDeclaredDefaultValue()
    {
        _database.ExecuteRaw("CREATE TABLE DefaultProbe (U INT DEFAULT 5, V INT)");

        ResultSet result = _database.Query(
            "SELECT COLUMN_NAME, COLUMN_DEFAULT FROM COLUMNS WHERE TABLE_NAME = 'DefaultProbe' ORDER BY ORDINAL_POSITION"
        );

        Assert.Equal(["5", null], result.Column(1));
    }

    [Fact]
    public void Columns_OfAUserView_AreListedWithTheViewName()
    {
        _database.ExecuteRaw(
            "CREATE VIEW CustomerNames AS SELECT FirstName, LastName FROM Customer"
        );

        ResultSet result = _database.Query(
            "SELECT COLUMN_NAME FROM COLUMNS WHERE TABLE_NAME = 'CustomerNames' ORDER BY ORDINAL_POSITION"
        );

        Assert.Equal(["FirstName", "LastName"], result.Column());
    }

    [Fact]
    public void Columns_CanBeJoinedWithTablesLikeInSqlServer()
    {
        ResultSet result = _database.Query(
            "SELECT t.TABLE_NAME, COUNT(*) FROM TABLES t INNER JOIN COLUMNS c ON c.TABLE_NAME = t.TABLE_NAME "
                + "WHERE t.TABLE_TYPE = 'BASE TABLE' GROUP BY t.TABLE_NAME ORDER BY t.TABLE_NAME"
        );

        Assert.Equal(["Customer", "OrderItem", "Orders", "Product"], result.Column());
        Assert.Equal(4, result.RowCount);
    }

    [Theory]
    [InlineData("playground")]
    [InlineData("analytics")]
    public void Views_AreAvailableInEveryScenario(string id)
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(id);

        Assert.True((long)database.Scalar("SELECT COUNT(*) FROM TABLES")! > 0);
        Assert.True((long)database.Scalar("SELECT COUNT(*) FROM COLUMNS")! > 0);
    }

    [Fact]
    public void Views_TableAndColumnCountsAgree()
    {
        Assert.Equal(
            _database.Scalar("SELECT COUNT(*) FROM (SELECT DISTINCT TABLE_NAME FROM COLUMNS) AS d"),
            _database.Scalar("SELECT COUNT(*) FROM TABLES")
        );
    }
}

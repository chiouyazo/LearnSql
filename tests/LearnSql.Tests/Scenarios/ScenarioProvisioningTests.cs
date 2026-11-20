using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Scenarios;

public class ScenarioProvisioningTests
{
    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_SchemaViewsAndSeedInOrder_Succeeds(string id)
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();

        database.Provision(id);

        Assert.True(
            (long)database.Scalar("SELECT COUNT(*) FROM TABLES")! > 0,
            "The scenario defines no tables."
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_EachScriptRunsOnItsOwnInTheDocumentedOrder(string id)
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        ScenarioScripts scripts = ScenarioData.Catalog.GetScripts(id);

        foreach (SqlScript script in scripts.InExecutionOrder)
        {
            Exception? failure = Record.Exception(() => database.ExecuteRaw(script.Content));
            Assert.True(failure is null, $"{script.Resource} failed: {failure?.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_LeavesNoForeignKeyViolations(string id)
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(id);

        Assert.Empty(database.ForeignKeyViolations());
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_EveryBaseTableContainsRows(string id)
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(id);
        ResultSet tables = database.Query(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
        );

        Assert.All(
            tables.Column(),
            (object? name) =>
                Assert.True(
                    (long)database.Scalar($"SELECT COUNT(*) FROM [{name}]")! > 0,
                    $"Table {name} of scenario {id} is empty."
                )
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_TwiceOnTheSameDatabase_Fails(string id)
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(id);

        Assert.ThrowsAny<SqliteException>(() => database.Provision(id));
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_TwoFreshDatabases_ContainTheSameData(string id)
    {
        using SqliteTestDatabase first = SqliteTestDatabase.ForScenario(id);
        using SqliteTestDatabase second = SqliteTestDatabase.ForScenario(id);
        ResultSet tables = first.Query(
            "SELECT TABLE_NAME FROM TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
        );

        foreach (object? name in tables.Column())
        {
            Assert.Equal(
                first.Scalar($"SELECT COUNT(*) FROM [{name}]"),
                second.Scalar($"SELECT COUNT(*) FROM [{name}]")
            );
        }
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Provision_ForeignKeyEnforcementIsActive(string id)
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario(id);

        Assert.Equal(1L, database.Scalar("SELECT * FROM pragma_foreign_keys"));
    }

    [Fact]
    public void Provision_UnknownScenario_ThrowsBeforeTouchingTheDatabase()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            database.Provision("missing")
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnknownScenario, exception.Kind);
        Assert.Equal(0L, database.Scalar("SELECT COUNT(*) FROM sqlite_master"));
    }

    [Fact]
    public void Starter_HasTheDocumentedShopTables()
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario("starter");

        string[] expected = ["Customer", "OrderItem", "Orders", "Product"];

        Assert.Equal(
            expected,
            database.Query("SELECT TABLE_NAME FROM TABLES").Column().Cast<string>().Order()
        );
    }

    [Fact]
    public void Starter_ContainsCustomersWithoutOrdersAndMissingValues()
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario("starter");

        Assert.True(
            (long)
                database.Scalar(
                    "SELECT COUNT(*) FROM Customer c WHERE NOT EXISTS (SELECT 1 FROM Orders o WHERE o.CustomerId = c.Id)"
                )! > 0
        );
        Assert.True((long)database.Scalar("SELECT COUNT(*) FROM Customer WHERE City IS NULL")! > 0);
    }

    [Fact]
    public void Analytics_HasAMonthlyTimeSeriesOfTwentyFourMonths()
    {
        using SqliteTestDatabase database = SqliteTestDatabase.ForScenario("analytics");

        Assert.Equal(
            24L,
            database.Scalar("SELECT COUNT(DISTINCT strftime('%Y-%m', OrderDate)) FROM Orders")
        );
    }
}

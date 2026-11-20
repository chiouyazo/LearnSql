using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class SchemaTests
{
    private static ColumnSchema Column(RelationSchema relation, string name)
    {
        return Assert.Single(
            relation.Columns,
            (ColumnSchema c) => string.Equals(c.Name, name, StringComparison.Ordinal)
        );
    }

    private static RelationSchema Relation(DatabaseSchema schema, string name)
    {
        RelationSchema? relation = schema.Find(name);
        Assert.NotNull(relation);
        return relation;
    }

    [Fact]
    public async Task GetSchemaAsync_OnAnEmptyDatabase_IsEmpty()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        DatabaseSchema schema = await database.GetSchemaAsync();

        Assert.Empty(schema.Relations);
        Assert.Empty(schema.Tables);
        Assert.Empty(schema.Views);
    }

    [Fact]
    public async Task GetSchemaAsync_ListsTheTablesOfTheStarterScenario()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        DatabaseSchema schema = await database.GetSchemaAsync();

        Assert.Equal(
            ["Customer", "OrderItem", "Orders", "Product"],
            schema
                .Tables.Where((RelationSchema t) => !t.IsSystem)
                .Select((RelationSchema t) => t.Name)
                .Order(StringComparer.Ordinal)
                .ToArray()
        );
    }

    [Fact]
    public async Task GetSchemaAsync_ColumnsCarryTypeNullabilityAndPrimaryKeyInformation()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        RelationSchema customer = Relation(await database.GetSchemaAsync(), "Customer");

        Assert.Equal(
            ["Id", "FirstName", "LastName", "City", "Email", "RegisteredOn"],
            customer.Columns.Select((ColumnSchema c) => c.Name).ToArray()
        );
        ColumnSchema id = Column(customer, "Id");
        Assert.Equal("INTEGER", id.DeclaredType);
        Assert.True(id.IsPrimaryKey);
        Assert.Equal(1, id.PrimaryKeyOrdinal);
        Assert.True(id.IsIdentity);
        ColumnSchema firstName = Column(customer, "FirstName");
        Assert.Equal("NVARCHAR(50)", firstName.DeclaredType);
        Assert.False(firstName.IsNullable);
        Assert.False(firstName.IsPrimaryKey);
        Assert.False(firstName.IsIdentity);
        Assert.Equal(0, firstName.PrimaryKeyOrdinal);
        ColumnSchema city = Column(customer, "City");
        Assert.True(city.IsNullable);
        Assert.Null(city.DefaultValue);
        Assert.Equal("DATE", Column(customer, "RegisteredOn").DeclaredType);
        Assert.Equal(
            "DECIMAL(8,2)",
            Column(Relation(await database.GetSchemaAsync(), "Product"), "Price").DeclaredType
        );
    }

    [Fact]
    public async Task GetSchemaAsync_ReportsDefaultValues()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (Id INT NOT NULL, Status NVARCHAR(10) NOT NULL DEFAULT 'new', Qty INT DEFAULT 5)"
        );

        RelationSchema table = Relation(await database.GetSchemaAsync(), "T");

        Assert.Equal("'new'", Column(table, "Status").DefaultValue);
        Assert.Equal("5", Column(table, "Qty").DefaultValue);
        Assert.Null(Column(table, "Id").DefaultValue);
    }

    [Fact]
    public async Task GetSchemaAsync_CompositePrimaryKeysKeepTheirOrder()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE Link (B INT NOT NULL, A INT NOT NULL, Note INT, CONSTRAINT PK_Link PRIMARY KEY (A, B))"
        );

        RelationSchema link = Relation(await database.GetSchemaAsync(), "Link");

        Assert.Equal(1, Column(link, "A").PrimaryKeyOrdinal);
        Assert.Equal(2, Column(link, "B").PrimaryKeyOrdinal);
        Assert.False(Column(link, "Note").IsPrimaryKey);
    }

    [Fact]
    public async Task GetSchemaAsync_ForeignKeysCarryConstraintNameAndReferencedColumn()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        DatabaseSchema schema = await database.GetSchemaAsync();

        ForeignKeySchema orders = Assert.Single(Relation(schema, "Orders").ForeignKeys);
        Assert.Equal("FK_Orders_Customer", orders.Name);
        Assert.Equal(["CustomerId"], orders.Columns);
        Assert.Equal("Customer", orders.ReferencedTable);
        Assert.Equal(["Id"], orders.ReferencedColumns);
        IReadOnlyList<ForeignKeySchema> items = Relation(schema, "OrderItem").ForeignKeys;
        Assert.Equal(2, items.Count);
        Assert.Equal(
            ["FK_OrderItem_Orders", "FK_OrderItem_Product"],
            items.Select((ForeignKeySchema k) => k.Name!).Order(StringComparer.Ordinal).ToArray()
        );
        Assert.Empty(Relation(schema, "Customer").ForeignKeys);
    }

    [Fact]
    public async Task GetSchemaAsync_ForeignKeyToTheImplicitPrimaryKey_ResolvesTheReferencedColumn()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE P (Id INT PRIMARY KEY); CREATE TABLE C (Id INT PRIMARY KEY, PId INT, "
                + "CONSTRAINT FK_C_P FOREIGN KEY (PId) REFERENCES P);"
        );

        ForeignKeySchema key = Assert.Single(
            Relation(await database.GetSchemaAsync(), "C").ForeignKeys
        );

        Assert.Equal(["Id"], key.ReferencedColumns);
        Assert.Equal("P", key.ReferencedTable);
    }

    [Fact]
    public async Task GetSchemaAsync_ForeignKeyActions_AreReported()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE P (Id INT PRIMARY KEY); CREATE TABLE C (Id INT PRIMARY KEY, PId INT, "
                + "CONSTRAINT FK_C_P FOREIGN KEY (PId) REFERENCES P (Id) ON DELETE CASCADE);"
        );

        ForeignKeySchema key = Assert.Single(
            Relation(await database.GetSchemaAsync(), "C").ForeignKeys
        );

        Assert.Equal("CASCADE", key.OnDelete);
        Assert.Equal("NO ACTION", key.OnUpdate);
    }

    [Fact]
    public async Task GetSchemaAsync_InformationSchemaViews_AreMarkedAsSystem()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        DatabaseSchema schema = await database.GetSchemaAsync();

        RelationSchema columns = Relation(schema, "COLUMNS");
        RelationSchema tables = Relation(schema, "TABLES");
        Assert.True(columns.IsSystem);
        Assert.True(tables.IsSystem);
        Assert.Equal(RelationKind.View, columns.Kind);
        Assert.Equal(RelationKind.View, tables.Kind);
        Assert.Contains(columns, schema.Views);
        Assert.Contains(tables, schema.Views);
        Assert.Contains(schema.Tables, (RelationSchema t) => !t.IsSystem);
        Assert.All(schema.Tables, (RelationSchema t) => Assert.False(t.IsSystem));
    }

    [Fact]
    public async Task GetSchemaAsync_UserObjects_AreNotMarkedAsSystem()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync(
            "CREATE TABLE Mine (a INT);\nGO\nCREATE VIEW MyView AS SELECT a FROM Mine"
        );

        DatabaseSchema schema = await database.GetSchemaAsync();

        Assert.False(Relation(schema, "Mine").IsSystem);
        Assert.False(Relation(schema, "MyView").IsSystem);
        Assert.Equal(RelationKind.View, Relation(schema, "MyView").Kind);
        Assert.Equal(
            ["COLUMNS", "TABLES"],
            schema
                .Relations.Where((RelationSchema r) => r.IsSystem)
                .Select((RelationSchema r) => r.Name)
                .Order(StringComparer.Ordinal)
                .ToArray()
        );
    }

    [Fact]
    public async Task GetSchemaAsync_InternalObjects_AreNotListed()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync("DECLARE @x INT; SET @x = 1;");

        DatabaseSchema schema = await database.GetSchemaAsync();

        Assert.Null(schema.Find("sqlite_sequence"));
        Assert.Null(schema.Find("__learnsql_variables"));
        Assert.All(
            schema.Relations,
            (RelationSchema r) =>
                Assert.False(r.Name.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase))
        );
    }

    [Fact]
    public async Task GetSchemaAsync_AfterCreateTable_ShowsTheNewTable()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        Assert.Null((await database.GetSchemaAsync()).Find("Fresh"));

        await database.ExecuteOkAsync(
            "CREATE TABLE Fresh (Id INT PRIMARY KEY, CustomerId INT, "
                + "CONSTRAINT FK_Fresh_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id))"
        );

        RelationSchema fresh = Relation(await database.GetSchemaAsync(), "Fresh");
        Assert.Equal(2, fresh.Columns.Count);
        Assert.Equal("FK_Fresh_Customer", Assert.Single(fresh.ForeignKeys).Name);
    }

    [Fact]
    public async Task GetSchemaAsync_AfterAlterTableAddColumn_ShowsTheColumn()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");
        Assert.Single(Relation(await database.GetSchemaAsync(), "T").Columns);

        await database.ExecuteOkAsync("ALTER TABLE T ADD b NVARCHAR(5) NULL");

        RelationSchema table = Relation(await database.GetSchemaAsync(), "T");
        Assert.Equal(["a", "b"], table.Columns.Select((ColumnSchema c) => c.Name).ToArray());
    }

    [Fact]
    public async Task GetSchemaAsync_AfterDropTable_HidesTheTable()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync("CREATE TABLE Gone (a INT)");
        Assert.NotNull((await database.GetSchemaAsync()).Find("Gone"));

        await database.ExecuteOkAsync("DROP TABLE Gone");

        Assert.Null((await database.GetSchemaAsync()).Find("Gone"));
    }

    [Fact]
    public async Task GetSchemaAsync_AfterAFailedScriptWithEarlierDdl_ReflectsTheActualState()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");
        await database.GetSchemaAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE Early (a INT); SELECT * FROM Missing"
        );

        Assert.False(result.Succeeded);
        Assert.NotNull((await database.GetSchemaAsync()).Find("Early"));
    }

    [Fact]
    public async Task GetSchemaAsync_AfterProvisioning_ReflectsTheNewScenario()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        Assert.NotNull((await database.GetSchemaAsync()).Find("Customer"));

        await database.ProvisionScenarioAsync("playground");

        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.NotNull(schema.Find("Country"));
        Assert.Equal(19, schema.Tables.Count((RelationSchema t) => !t.IsSystem));
    }

    [Fact]
    public async Task GetSchemaAsync_Find_IsCaseInsensitive()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        DatabaseSchema schema = await database.GetSchemaAsync();

        Assert.Same(schema.Find("Customer"), schema.Find("CUSTOMER"));
        Assert.Null(schema.Find("NoSuchRelation"));
    }

    [Fact]
    public async Task GetSchemaAsync_CancelledToken_Throws()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            database.GetSchemaAsync(source.Token)
        );
    }
}

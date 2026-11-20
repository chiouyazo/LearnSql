using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class SchemaTypingTests
{
    [Theory]
    [InlineData("INT", ValueKind.Numeric)]
    [InlineData("INTEGER", ValueKind.Numeric)]
    [InlineData("BIGINT", ValueKind.Numeric)]
    [InlineData("DECIMAL(8,2)", ValueKind.Numeric)]
    [InlineData("NUMERIC", ValueKind.Numeric)]
    [InlineData("FLOAT", ValueKind.Numeric)]
    [InlineData("REAL", ValueKind.Numeric)]
    [InlineData("DOUBLE", ValueKind.Numeric)]
    [InlineData("MONEY", ValueKind.Numeric)]
    [InlineData("BIT", ValueKind.Numeric)]
    [InlineData("VARCHAR(50)", ValueKind.Text)]
    [InlineData("NVARCHAR(MAX)", ValueKind.Text)]
    [InlineData("CHAR(3)", ValueKind.Text)]
    [InlineData("TEXT", ValueKind.Text)]
    [InlineData("UNIQUEIDENTIFIER", ValueKind.Text)]
    [InlineData("DATE", ValueKind.DateTime)]
    [InlineData("DATETIME", ValueKind.DateTime)]
    [InlineData("DATETIME2(7)", ValueKind.DateTime)]
    [InlineData("TIME", ValueKind.DateTime)]
    [InlineData("varchar(10)", ValueKind.Text)]
    [InlineData("BLOB", ValueKind.Binary)]
    [InlineData("VARBINARY(MAX)", ValueKind.Binary)]
    [InlineData("IMAGE", ValueKind.Binary)]
    [InlineData("", ValueKind.Unknown)]
    [InlineData("   ", ValueKind.Unknown)]
    [InlineData(null, ValueKind.Unknown)]
    [InlineData("SOMETHING", ValueKind.Unknown)]
    public void DeclaredTypeClassifier_Classify_MapsDeclaredTypesToKinds(
        string? declaredType,
        ValueKind expected
    )
    {
        Assert.Equal(expected, DeclaredTypeClassifier.Classify(declaredType));
    }

    [Fact]
    public void Reader_ReadsTablesAndViewsWithColumnKindsInDeclarationOrder()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        database.ExecuteRaw(
            "CREATE TABLE Item (Id INTEGER PRIMARY KEY, Name NVARCHAR(20), Price DECIMAL(8,2), Added DATE, Raw BLOB, Untyped);"
                + "CREATE VIEW ItemNames AS SELECT Name FROM Item;"
        );

        ISchemaCatalog catalog = database.ReadSchema();

        Assert.True(catalog.TryGetRelation("Item", out RelationShape? table));
        Assert.Equal(
            [
                new ColumnInfo("Id", ValueKind.Numeric),
                new ColumnInfo("Name", ValueKind.Text),
                new ColumnInfo("Price", ValueKind.Numeric),
                new ColumnInfo("Added", ValueKind.DateTime),
                new ColumnInfo("Raw", ValueKind.Binary),
                new ColumnInfo("Untyped", ValueKind.Unknown),
            ],
            table!.Columns
        );
        Assert.True(catalog.TryGetRelation("ItemNames", out RelationShape? view));
        Assert.Equal([new ColumnInfo("Name", ValueKind.Text)], view!.Columns);
    }

    [Fact]
    public void Catalog_LookupIsCaseInsensitiveForRelationsAndColumns()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        database.ExecuteRaw("CREATE TABLE Item (Name NVARCHAR(20));");

        ISchemaCatalog catalog = database.ReadSchema();

        Assert.True(catalog.TryGetRelation("ITEM", out RelationShape? shape));
        Assert.True(shape!.TryGetColumn("nAmE", out ColumnInfo? column));
        Assert.Equal("Name", column!.Name);
    }

    [Fact]
    public void Catalog_UnknownRelationOrColumn_IsNotFound()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        database.ExecuteRaw("CREATE TABLE Item (Name NVARCHAR(20));");

        ISchemaCatalog catalog = database.ReadSchema();
        catalog.TryGetRelation("Item", out RelationShape? shape);

        Assert.False(catalog.TryGetRelation("Missing", out RelationShape? missing));
        Assert.Null(missing);
        Assert.False(shape!.TryGetColumn("Missing", out ColumnInfo? column));
        Assert.Null(column);
    }

    [Fact]
    public void Reader_OnEmptyDatabase_ReturnsAnEmptyCatalog()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();

        Assert.False(database.ReadSchema().TryGetRelation("anything", out RelationShape? _));
    }

    [Fact]
    public void EmptyCatalog_KnowsNoRelation()
    {
        Assert.False(SchemaCatalog.Empty.TryGetRelation("Item", out RelationShape? _));
    }

    [Fact]
    public void Catalog_WithDuplicateRelationNames_KeepsTheLastDefinition()
    {
        SchemaCatalog catalog = new SchemaCatalog([
            new KeyValuePair<string, RelationShape>(
                "T",
                new RelationShape([new ColumnInfo("a", ValueKind.Text)])
            ),
            new KeyValuePair<string, RelationShape>(
                "t",
                new RelationShape([new ColumnInfo("b", ValueKind.Numeric)])
            ),
        ]);

        catalog.TryGetRelation("T", out RelationShape? shape);

        Assert.Equal("b", Assert.Single(shape!.Columns).Name);
    }

    [Fact]
    public void Translate_WithTableAddedAfterwards_UsesTheFreshCatalogOnTheNextRun()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        database.RunSuccessfully(
            "CREATE TABLE T (A VARCHAR(5), B VARCHAR(5)); INSERT INTO T VALUES ('x', 'y')"
        );

        Assert.Equal("xy", database.Scalar("SELECT A + B FROM T"));
    }

    [Fact]
    public void Translate_WithoutSchemaForTheTable_FallsBackToAdditionForUnknownColumns()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        database.ExecuteRaw(
            "CREATE TABLE T (A VARCHAR(5), B VARCHAR(5)); INSERT INTO T VALUES ('1', '2');"
        );

        string sql = TSqlDialectTranslator
            .CreateDefault()
            .Translate("SELECT A + B FROM T")
            .Batches[0]
            .Statements[0]
            .Sql;

        Assert.Equal("SELECT A + B FROM T", sql);
    }
}

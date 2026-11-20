using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Translation;

public class SqliteErrorMapperTests
{
    [Fact]
    public void Match_WrapperFormat_UnwrapsTheMessageAndMapsTheType()
    {
        SqliteError error = SqliteErrorMapper.Match("SQLite Error 1: 'no such table: Customer'.");

        Assert.Equal(SqliteErrorType.NoSuchTable, error.Type);
        Assert.Equal("no such table: Customer", error.Message);
        Assert.Equal("Customer", error.Arguments["Table"]);
    }

    [Fact]
    public void Match_WrapperFormatWithMultilineMessage_StillUnwraps()
    {
        SqliteError error = SqliteErrorMapper.Match("SQLite Error 1: 'no such table: a\nb'.");

        Assert.Equal(SqliteErrorType.NoSuchTable, error.Type);
        Assert.Equal("a\nb", error.Arguments["Table"]);
    }

    [Fact]
    public void Match_PlainMessageWithoutWrapper_IsMapped()
    {
        SqliteError error = SqliteErrorMapper.Match("no such table: Customer");

        Assert.Equal(SqliteErrorType.NoSuchTable, error.Type);
    }

    [Theory]
    [InlineData("NO SUCH TABLE: Customer")]
    [InlineData("No Such Table: Customer")]
    public void Match_DifferentCasing_IsStillMapped(string message)
    {
        Assert.Equal(SqliteErrorType.NoSuchTable, SqliteErrorMapper.Match(message).Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("something completely different")]
    [InlineData("SQLite Error 1: 'completely unknown'.")]
    public void Match_UnknownMessage_ReturnsUnknownWithoutArguments(string message)
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.Unknown, error.Type);
        Assert.Empty(error.Arguments);
    }

    [Fact]
    public void Match_UnknownWrappedMessage_KeepsTheUnwrappedText()
    {
        SqliteError error = SqliteErrorMapper.Match("SQLite Error 1: 'completely unknown'.");

        Assert.Equal("completely unknown", error.Message);
    }

    [Fact]
    public void Match_NullMessage_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SqliteErrorMapper.Match(null!));
    }

    [Theory]
    [InlineData("user already exists")]
    [InlineData("the file already exists")]
    [InlineData("already exists")]
    [InlineData("table already exists")]
    [InlineData("index already exists")]
    [InlineData("column Foo already exists")]
    public void Match_ArbitraryTextAroundAlreadyExists_IsNotAlreadyExists(string message)
    {
        Assert.NotEqual(SqliteErrorType.AlreadyExists, SqliteErrorMapper.Match(message).Type);
    }

    [Theory]
    [InlineData("table Foo already exists", "table", "Foo")]
    [InlineData("view Bar already exists", "view", "Bar")]
    public void Match_TableOrViewAlreadyExists_CapturesObjectTypeAndName(
        string message,
        string objectType,
        string name
    )
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.AlreadyExists, error.Type);
        Assert.Equal(objectType, error.Arguments["ObjectType"]);
        Assert.Equal(name, error.Arguments["Name"]);
    }

    [Theory]
    [InlineData("cannot alter the thing")]
    [InlineData("cannot rename columns of table \"t\"")]
    [InlineData("cannot drop column from table \"t\"")]
    [InlineData("cannot edit constraints of index \"i\"")]
    [InlineData("cannot")]
    [InlineData("cannot connect to the server")]
    public void Match_ArbitraryCannotText_IsNotCannotAlterObject(string message)
    {
        Assert.NotEqual(SqliteErrorType.CannotAlterObject, SqliteErrorMapper.Match(message).Type);
    }

    [Theory]
    [InlineData("cannot rename columns of view \"v\"", "rename columns of", "view", "v")]
    [InlineData(
        "cannot drop column from virtual table \"t\"",
        "drop column from",
        "virtual table",
        "t"
    )]
    [InlineData("cannot edit constraints of view \"x\"", "edit constraints of", "view", "x")]
    public void Match_AlterOnViewOrVirtualTable_CapturesActionObjectTypeAndName(
        string message,
        string action,
        string objectType,
        string name
    )
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.CannotAlterObject, error.Type);
        Assert.Equal(action, error.Arguments["Action"]);
        Assert.Equal(objectType, error.Arguments["ObjectType"]);
        Assert.Equal(name, error.Arguments["Name"]);
    }

    [Theory]
    [InlineData("no such index: Foo")]
    [InlineData("no such index: \"Foo\"")]
    [InlineData("SQLite Error 1: 'no such index: Foo'.")]
    public void Match_NoSuchIndexVariants_ResolveToTheSameTypeAndName(string message)
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.NoSuchIndex, error.Type);
        Assert.Equal("Foo", error.Arguments["Index"]);
    }

    [Theory]
    [InlineData("no such column: Foo")]
    [InlineData("no such column: \"Foo\"")]
    [InlineData("no such column: \"Foo\" - should this be a string literal in single-quotes?")]
    [InlineData("SQLite Error 1: 'no such column: Foo'.")]
    public void Match_NoSuchColumnVariants_ResolveToTheSameTypeAndName(string message)
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.NoSuchColumn, error.Type);
        Assert.Equal("Foo", error.Arguments["Column"]);
    }

    [Theory]
    [InlineData("unrecognized token: \"@\"", "@")]
    [InlineData("unrecognized token: \"'abc\"", "'abc")]
    [InlineData("SQLite Error 1: 'unrecognized token: \"#\"'.", "#")]
    public void Match_UnrecognizedTokenVariants_CaptureTheToken(string message, string token)
    {
        SqliteError error = SqliteErrorMapper.Match(message);

        Assert.Equal(SqliteErrorType.UnrecognizedToken, error.Type);
        Assert.Equal(token, error.Arguments["Token"]);
    }

    [Fact]
    public void Match_NearSyntaxError_CapturesTheNearToken()
    {
        SqliteError error = SqliteErrorMapper.Match("near \"FROM\": syntax error");

        Assert.Equal(SqliteErrorType.NearSyntaxError, error.Type);
        Assert.Equal("FROM", error.Arguments["Near"]);
    }

    [Theory]
    [InlineData("SELECT Missing FROM Parent", SqliteErrorType.NoSuchColumn, "Column", "Missing")]
    [InlineData("SELECT * FROM Missing", SqliteErrorType.NoSuchTable, "Table", "Missing")]
    [InlineData("DROP INDEX Missing", SqliteErrorType.NoSuchIndex, "Index", "Missing")]
    [InlineData("SELECT 'abc", SqliteErrorType.UnrecognizedToken, "Token", "'abc")]
    [InlineData("SELECT 1 +", SqliteErrorType.IncompleteInput, "", "")]
    public void Match_RealSqliteMessage_ResolvesToTheExpectedType(
        string failingSql,
        SqliteErrorType expectedType,
        string argumentName,
        string argumentValue
    )
    {
        SqliteException exception = SqliteFailure.Capture(SqliteFailure.ShopSetup, failingSql);

        SqliteError error = SqliteErrorMapper.Match(exception.Message);

        Assert.Equal(expectedType, error.Type);
        if (argumentName.Length > 0)
            Assert.Equal(argumentValue, error.Arguments[argumentName]);
    }

    [Fact]
    public void Match_AmbiguousColumnFromRealSqlite_CapturesTheColumn()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "SELECT Id FROM Parent JOIN Child ON Parent.Id = Child.ParentId"
        );

        SqliteError error = SqliteErrorMapper.Match(exception.Message);

        Assert.Equal(SqliteErrorType.AmbiguousColumnName, error.Type);
        Assert.Equal("Id", error.Arguments["Column"]);
    }

    [Fact]
    public void Match_ColumnCountMismatchFromRealSqlite_IsRecognized()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Parent VALUES (5, 'X', 'extra')"
        );

        SqliteError error = SqliteErrorMapper.Match(exception.Message);

        Assert.Equal(SqliteErrorType.TableHasColumnsButValuesWereSupplied, error.Type);
    }

    [Fact]
    public void Match_UnknownColumnInInsertFromRealSqlite_IsRecognized()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Parent (Id, Missing) VALUES (5, 'X')"
        );

        SqliteError error = SqliteErrorMapper.Match(exception.Message);

        Assert.Equal(SqliteErrorType.TableHasNoColumnNamed, error.Type);
        Assert.Equal("Parent", error.Arguments["Table"]);
        Assert.Equal("Missing", error.Arguments["Column"]);
    }

    [Fact]
    public void Match_NoTablesSpecifiedFromRealSqlite_IsRecognized()
    {
        SqliteException exception = SqliteFailure.Capture("", "SELECT *");

        SqliteError error = SqliteErrorMapper.Match(exception.Message);

        Assert.Equal(SqliteErrorType.NoTablesSpecified, error.Type);
    }
}

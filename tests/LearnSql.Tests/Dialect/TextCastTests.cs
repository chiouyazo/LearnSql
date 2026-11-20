using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class TextCastTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> Cases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CAST('a' AS CHAR(3))", "a  " },
            { "SELECT CAST('abc' AS CHAR(3))", "abc" },
            { "SELECT CAST('abcdef' AS CHAR(3))", "abc" },
            { "SELECT CAST('ab' AS NCHAR(4))", "ab  " },
            { "SELECT CAST('abcdef' AS NCHAR(2))", "ab" },
            { "SELECT CAST('' AS CHAR(2))", "  " },
            { "SELECT CAST(5 AS CHAR(3))", "5  " },
            { "SELECT CAST(123456 AS CHAR(4))", "1234" },
            { "SELECT CAST(1.5 AS CHAR(5))", "1.5  " },
            { "SELECT CAST('ab' AS VARCHAR(5))", "ab" },
            { "SELECT CAST('ab' AS NVARCHAR(5))", "ab" },
            { "SELECT CAST('abcdef' AS VARCHAR(3))", "abc" },
            { "SELECT CAST('abcdef' AS NVARCHAR(3))", "abc" },
            { "SELECT CAST('abcdef' AS VARCHAR(MAX))", "abcdef" },
            { "SELECT CAST('abcdef' AS NVARCHAR(MAX))", "abcdef" },
            { "SELECT CAST(NULL AS CHAR(3))", null },
            { "SELECT CAST(NULL AS NCHAR(3))", null },
            { "SELECT CAST(NULL AS VARCHAR(3))", null },
            { "SELECT CONVERT(CHAR(5), 'ab')", "ab   " },
            { "SELECT CONVERT(VARCHAR(5), 'ab')", "ab" },
            { "SELECT CONVERT(CHAR(10), CAST('2024-03-15' AS DATE), 103)", "15/03/2024" },
            { "SELECT CONVERT(CHAR(12), CAST('2024-03-15' AS DATE), 103)", "15/03/2024  " },
            { "SELECT CAST('a' AS CHAR(3)) + '|'", "a  |" },
            { "DECLARE @c CHAR(3) = 'a'; SELECT @c", "a  " },
            { "DECLARE @c CHAR(3) = 'a'; SET @c = 'xy'; SELECT @c", "xy " },
            { "DECLARE @c NCHAR(2) = 'abc'; SELECT @c", "ab" },
        };

    [Theory]
    [MemberData(nameof(Cases))]
    public void CastToText_PadsFixedLengthTypesAndTruncatesTheOthers(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Fact]
    public void CastToChar_WithoutLength_UsesTheConversionDefaultOfThirty()
    {
        object? value = _database.Scalar("SELECT CAST('a' AS CHAR)");

        Assert.Equal("a" + new string(' ', 29), value);
    }

    [Fact]
    public void CastToVarchar_WithoutLength_TruncatesToThirtyCharacters()
    {
        object? value = _database.Scalar(
            "SELECT CAST('0123456789012345678901234567890123456789' AS VARCHAR)"
        );

        Assert.Equal("012345678901234567890123456789", value);
    }

    [Fact]
    public void DeclareVarchar_WithoutLength_HasLengthOne()
    {
        Assert.Equal("a", _database.Scalar("DECLARE @s VARCHAR = 'abc'; SELECT @s"));
    }

    [Fact]
    public void SetVarchar_WithoutLength_TruncatesToOneCharacter()
    {
        Assert.Equal("x", _database.Scalar("DECLARE @s VARCHAR = 'a'; SET @s = 'xyz'; SELECT @s"));
    }

    [Fact]
    public void CastToChar_KeepsTrailingSpacesInTheStoredValue()
    {
        _database.ExecuteRaw("CREATE TABLE P (C CHAR(5))");
        _database.RunSuccessfully("INSERT INTO P VALUES (CAST('ab' AS CHAR(5)))");

        Assert.Equal("ab   ", _database.Scalar("SELECT C FROM P"));
    }

    [Fact]
    public void CastToChar_ThenLen_IgnoresTheTrailingSpaces()
    {
        Assert.Equal(1L, _database.Scalar("SELECT LEN(CAST('a' AS CHAR(3)))"));
    }

    [Fact]
    public void CastToChar_OverTableColumn_PadsEveryRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE C (Raw VARCHAR(10)); INSERT INTO C VALUES ('a'), ('bb'), (NULL);"
        );

        ResultSet result = _database.Query("SELECT CAST(Raw AS CHAR(3)) FROM C ORDER BY rowid");

        Assert.Equal(["a  ", "bb ", null], result.Column());
    }
}

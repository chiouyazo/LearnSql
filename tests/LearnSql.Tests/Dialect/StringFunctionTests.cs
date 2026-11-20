using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class StringFunctionTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> Cases() =>
        new TheoryData<string, object?>
        {
            { "SELECT LEN('abc')", 3L },
            { "SELECT LEN('ab  ')", 2L },
            { "SELECT LEN('  ab')", 4L },
            { "SELECT LEN('')", 0L },
            { "SELECT LEN('   ')", 0L },
            { "SELECT LEN(123)", 3L },
            { "SELECT LEN(NULL)", null },
            { "SELECT LEFT('abcdef', 2)", "ab" },
            { "SELECT LEFT('abc', 10)", "abc" },
            { "SELECT LEFT('abc', 0)", "" },
            { "SELECT LEFT(NULL, 2)", null },
            { "SELECT RIGHT('abcdef', 2)", "ef" },
            { "SELECT RIGHT('abc', 10)", "abc" },
            { "SELECT RIGHT('abc', 0)", "" },
            { "SELECT SUBSTRING('abcdef', 2, 3)", "bcd" },
            { "SELECT SUBSTRING('abcdef', 3, 100)", "cdef" },
            { "SELECT SUBSTRING('abcdef', 1, 0)", "" },
            { "SELECT SUBSTRING('abcdef', 10, 2)", "" },
            { "SELECT SUBSTRING('abcdef', -1, 4)", "ab" },
            { "SELECT SUBSTRING('abcdef', 0, 3)", "ab" },
            { "SELECT SUBSTRING('abcdef', -5, 3)", "" },
            { "SELECT SUBSTRING('abcdef', -2, 100)", "abcdef" },
            { "SELECT SUBSTRING(NULL, 1, 2)", null },
            { "SELECT SUBSTRING('abc', NULL, 2)", null },
            { "SELECT CHARINDEX('c', 'abcabc')", 3L },
            { "SELECT CHARINDEX('C', 'abcabc')", 3L },
            { "SELECT CHARINDEX('c', 'abcabc', 4)", 6L },
            { "SELECT CHARINDEX('c', 'abcabc', 7)", 0L },
            { "SELECT CHARINDEX('z', 'abc')", 0L },
            { "SELECT CHARINDEX('', 'abc')", 0L },
            { "SELECT CHARINDEX('c', 'abcabc', -5)", 3L },
            { "SELECT CHARINDEX(NULL, 'abc')", null },
            { "SELECT REPLICATE('ab', 3)", "ababab" },
            { "SELECT REPLICATE('ab', 0)", "" },
            { "SELECT REPLICATE('ab', -1)", null },
            { "SELECT REPLICATE(NULL, 3)", null },
            { "SELECT SPACE(3)", "   " },
            { "SELECT SPACE(0)", "" },
            { "SELECT SPACE(-1)", null },
            { "SELECT STUFF('abcdef', 2, 3, 'X')", "aXef" },
            { "SELECT STUFF('abcdef', 1, 0, 'X')", "Xabcdef" },
            { "SELECT STUFF('abcdef', 5, 100, 'X')", "abcdX" },
            { "SELECT STUFF('abcdef', 2, 2, NULL)", "adef" },
            { "SELECT STUFF('abcdef', 0, 1, 'X')", null },
            { "SELECT STUFF('abcdef', 7, 1, 'X')", null },
            { "SELECT STUFF('abcdef', 2, -1, 'X')", null },
            { "SELECT REVERSE('abc')", "cba" },
            { "SELECT REVERSE('')", "" },
            { "SELECT REVERSE(NULL)", null },
            { "SELECT REPLACE('aAbB', 'a', 'x')", "xxbB" },
            { "SELECT REPLACE('abc', '', 'x')", "abc" },
            { "SELECT REPLACE('abc', 'b', '')", "ac" },
            { "SELECT REPLACE('abc', NULL, 'x')", null },
            { "SELECT UPPER('abc')", "ABC" },
            { "SELECT LOWER('ABC')", "abc" },
            { "SELECT UPPER('äöü')", "ÄÖÜ" },
            { "SELECT LOWER('ÄÖÜ')", "äöü" },
            { "SELECT UPPER(NULL)", null },
            { "SELECT UPPER(12)", "12" },
            { "SELECT ASCII('A')", 65L },
            { "SELECT ASCII('abc')", 97L },
            { "SELECT ASCII('')", null },
            { "SELECT ASCII(NULL)", null },
        };

    [Theory]
    [MemberData(nameof(Cases))]
    public void StringFunction_BehavesLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData(
        "SELECT LEFT('abc', -1)",
        "Invalid length parameter passed to the LEFT or SUBSTRING function."
    )]
    [InlineData(
        "SELECT RIGHT('abc', -1)",
        "Invalid length parameter passed to the RIGHT function."
    )]
    [InlineData(
        "SELECT SUBSTRING('abc', 1, -1)",
        "Invalid length parameter passed to the substring function."
    )]
    public void StringFunction_NegativeLength_FailsWithAnInvalidLengthMessage(
        string sql,
        string expectedMessage
    )
    {
        ScriptOutcome outcome = _database.Run(sql);

        Assert.NotNull(outcome.Error);
        Assert.Equal(expectedMessage, outcome.Error.Error.Message);
    }

    [Fact]
    public void Len_OverTableColumn_IgnoresTrailingSpacesOfEachRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE W (Word VARCHAR(10)); INSERT INTO W VALUES ('a  '), ('bb'), ('  c');"
        );

        ResultSet result = _database.Query("SELECT LEN(Word) FROM W ORDER BY rowid");

        Assert.Equal([1L, 2L, 3L], result.Column());
    }

    [Fact]
    public void Concatenation_OfFunctionResults_IsAppliedAsText()
    {
        Assert.Equal("ab-cd", _database.Scalar("SELECT LEFT('abx', 2) + '-' + RIGHT('xcd', 2)"));
    }
}

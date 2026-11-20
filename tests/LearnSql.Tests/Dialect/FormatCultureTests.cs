using System.Globalization;
using LearnSql.Sql.Dialect.Functions;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class FormatCultureTests : IDisposable
{
    private const string Date = "CAST('2024-03-15 10:20:30' AS DATETIME)";

    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, string> ExpectedNumbers() =>
        new TheoryData<string, string>
        {
            { "en-US", "1,234.50" },
            { "en-GB", "1,234.50" },
            { "de-DE", "1.234,50" },
            { "fr-FR", "1 234,50" },
            { "es-ES", "1.234,50" },
            { "it-IT", "1.234,50" },
            { "en", "1,234.50" },
            { "de", "1.234,50" },
            { "fr", "1 234,50" },
            { "es", "1.234,50" },
            { "it", "1.234,50" },
            { "DE-de", "1.234,50" },
            { "FR-FR", "1 234,50" },
            { " de-DE ", "1.234,50" },
        };

    public static TheoryData<string, string, string> DateCases() =>
        new TheoryData<string, string, string>
        {
            { "en-US", "d", "3/15/2024" },
            { "en-GB", "d", "15/03/2024" },
            { "de-DE", "d", "15.03.2024" },
            { "fr-FR", "d", "15/03/2024" },
            { "es-ES", "d", "15/3/2024" },
            { "it-IT", "d", "15/03/2024" },
            { "en-US", "D", "Friday, March 15, 2024" },
            { "en-GB", "D", "Friday, 15 March 2024" },
            { "de-DE", "D", "Freitag, 15. März 2024" },
            { "fr-FR", "D", "vendredi 15 mars 2024" },
            { "es-ES", "D", "viernes, 15 de marzo de 2024" },
            { "it-IT", "D", "venerdì 15 marzo 2024" },
            { "en-US", "MMMM", "March" },
            { "de-DE", "MMMM", "März" },
            { "fr-FR", "MMMM", "mars" },
            { "es-ES", "MMMM", "marzo" },
            { "it-IT", "MMMM", "marzo" },
            { "en-US", "dddd", "Friday" },
            { "de-DE", "dddd", "Freitag" },
            { "fr-FR", "dddd", "vendredi" },
            { "es-ES", "dddd", "viernes" },
            { "it-IT", "dddd", "venerdì" },
            { "en-US", "ddd", "Fri" },
            { "de-DE", "ddd", "Fr" },
            { "en-US", "MMM", "Mar" },
            { "de-DE", "MMM", "Mär" },
            { "en-US", "t", "10:20 AM" },
            { "en-US", "T", "10:20:30 AM" },
            { "de-DE", "t", "10:20" },
            { "de-DE", "T", "10:20:30" },
            { "en-US", "dd/MM/yyyy", "15/03/2024" },
            { "de-DE", "dd/MM/yyyy", "15.03.2024" },
            { "de-DE", "dd.MM.yyyy HH:mm", "15.03.2024 10:20" },
            { "de-DE", "HH:mm", "10:20" },
            { "en-US", "M", "March 15" },
            { "de-DE", "M", "15. März" },
            { "fr-FR", "Y", "mars 2024" },
            { "en-US", "g", "3/15/2024 10:20 AM" },
            { "de-DE", "g", "15.03.2024 10:20" },
            { "en-US", "F", "Friday, March 15, 2024 10:20:30 AM" },
            { "de-DE", "F", "Freitag, 15. März 2024 10:20:30" },
        };

    [Theory]
    [MemberData(nameof(ExpectedNumbers))]
    public void Format_Number_UsesTheSeparatorsOfTheCulture(string culture, string expected)
    {
        Assert.Equal(expected, _database.Scalar($"SELECT FORMAT(1234.5, 'N2', '{culture}')"));
    }

    [Theory]
    [MemberData(nameof(DateCases))]
    public void Format_Date_UsesTheNamesAndPatternsOfTheCulture(
        string culture,
        string pattern,
        string expected
    )
    {
        Assert.Equal(
            expected,
            _database.Scalar($"SELECT FORMAT({Date}, '{pattern}', '{culture}')")
        );
    }

    [Theory]
    [InlineData("de-DE", "-1.234,50")]
    [InlineData("en-US", "-1,234.50")]
    [InlineData("fr-FR", "-1 234,50")]
    public void Format_NegativeNumber_KeepsTheSignInFront(string culture, string expected)
    {
        Assert.Equal(expected, _database.Scalar($"SELECT FORMAT(-1234.5, 'N2', '{culture}')"));
    }

    [Theory]
    [InlineData("de-DE", "N0", "1.234.567")]
    [InlineData("en-US", "N0", "1,234,567")]
    [InlineData("de-DE", "N3", "1.234.567,000")]
    [InlineData("de-DE", "F2", "1234567,00")]
    [InlineData("en-US", "F2", "1234567.00")]
    [InlineData("de-DE", "D", "1234567")]
    [InlineData("de-DE", "X", "12D687")]
    [InlineData("de-DE", "#,##0.00", "1.234.567,00")]
    [InlineData("en-US", "0.0", "1234567.0")]
    public void Format_Integer_UsesTheCultureForGroupingAndDecimals(
        string culture,
        string pattern,
        string expected
    )
    {
        Assert.Equal(
            expected,
            _database.Scalar($"SELECT FORMAT(1234567, '{pattern}', '{culture}')")
        );
    }

    [Theory]
    [InlineData("en-US", "$1,234.50")]
    [InlineData("en-GB", "£1,234.50")]
    public void Format_Currency_UsesTheSymbolOfTheCulture(string culture, string expected)
    {
        Assert.Equal(expected, _database.Scalar($"SELECT FORMAT(1234.5, 'C', '{culture}')"));
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("es-ES")]
    [InlineData("it-IT")]
    public void Format_Currency_InEuroCultures_EndsWithTheEuroSign(string culture)
    {
        string text = (string)_database.Scalar($"SELECT FORMAT(1234.5, 'C', '{culture}')")!;

        Assert.EndsWith("€", text);
        Assert.Contains(",50", text);
    }

    [Fact]
    public void Format_NegativeCurrency_UsesTheNegativePatternOfTheCulture()
    {
        Assert.Equal("($1,234.50)", _database.Scalar("SELECT FORMAT(-1234.5, 'C', 'en-US')"));
        Assert.Equal("-£1,234.50", _database.Scalar("SELECT FORMAT(-1234.5, 'C', 'en-GB')"));
        string german = (string)_database.Scalar("SELECT FORMAT(-1234.5, 'C', 'de-DE')")!;
        Assert.StartsWith("-1.234,50", german);
        Assert.EndsWith("€", german);
    }

    [Fact]
    public void Format_Percent_UsesThePatternOfTheCulture()
    {
        Assert.Equal("25.6%", _database.Scalar("SELECT FORMAT(0.256, 'P1', 'en-US')"));
        string german = (string)_database.Scalar("SELECT FORMAT(0.256, 'P1', 'de-DE')")!;
        Assert.StartsWith("25,6", german);
        Assert.EndsWith("%", german);
    }

    [Fact]
    public void Format_WithoutCulture_UsesEnglishUs()
    {
        Assert.Equal("3/15/2024", _database.Scalar($"SELECT FORMAT({Date}, 'd')"));
        Assert.Equal("1,234.50", _database.Scalar("SELECT FORMAT(1234.5, 'N2')"));
        Assert.Equal("$1,234.50", _database.Scalar("SELECT FORMAT(1234.5, 'C')"));
    }

    [Fact]
    public void Format_WithNullCulture_UsesEnglishUs()
    {
        Assert.Equal("3/15/2024", _database.Scalar($"SELECT FORMAT({Date}, 'd', NULL)"));
    }

    [Fact]
    public void Format_WithEmptyCulture_UsesTheInvariantCulture()
    {
        Assert.Equal("03/15/2024", _database.Scalar($"SELECT FORMAT({Date}, 'd', '')"));
        Assert.Equal("1,234.50", _database.Scalar("SELECT FORMAT(1234.5, 'N2', '')"));
    }

    [Theory]
    [InlineData("xx-INVALID")]
    [InlineData("de_DE")]
    [InlineData("klingon")]
    [InlineData("ja-JP")]
    [InlineData("de-XX")]
    [InlineData("1")]
    public void Format_WithUnsupportedCulture_FailsWithError9818(string culture)
    {
        SqlErrorAssert.Raised(
            _database.Run($"SELECT FORMAT(1, 'N2', '{culture}')"),
            9818,
            16,
            1,
            $"The culture parameter '{culture}' provided in the function call is not supported."
        );
    }

    [Fact]
    public void Format_WithUnsupportedCulture_AlsoFailsForDates()
    {
        SqlErrorAssert.Raised(
            _database.Run($"SELECT FORMAT({Date}, 'd', 'xx-INVALID')"),
            9818,
            16,
            1,
            "The culture parameter 'xx-INVALID' provided in the function call is not supported."
        );
    }

    [Fact]
    public void Format_OfNull_ReturnsNullEvenWithUnsupportedCulture()
    {
        Assert.Null(_database.Scalar("SELECT FORMAT(NULL, 'N2', 'xx-INVALID')"));
    }

    [Fact]
    public void Format_UnsupportedCulture_FailsWhenTheRowIsEvaluated()
    {
        _database.ExecuteRaw("CREATE TABLE F (V INT); INSERT INTO F VALUES (1), (2);");

        SqlErrorAssert.Raised(
            _database.Run("SELECT FORMAT(V, 'N2', 'xx') FROM F"),
            9818,
            16,
            1,
            "The culture parameter 'xx' provided in the function call is not supported."
        );
    }

    [Fact]
    public void Format_CultureFromColumn_IsResolvedPerRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE F (Lang VARCHAR(10)); INSERT INTO F VALUES ('en-US'), ('de-DE');"
        );

        ResultSet result = _database.Query(
            "SELECT FORMAT(1234.5, 'N2', Lang) FROM F ORDER BY rowid"
        );

        Assert.Equal(["1,234.50", "1.234,50"], result.Column());
    }

    [Fact]
    public void SupportedNames_AllResolve_AndEachIsAcceptedBySql()
    {
        foreach (string name in FormatCultures.SupportedNames)
        {
            Assert.True(FormatCultures.TryGet(name, out CultureInfo? culture));
            Assert.NotNull(culture);
            Assert.NotNull(_database.Scalar($"SELECT FORMAT(1234.5, 'N2', '{name}')"));
        }
    }

    [Fact]
    public void Cultures_AreReadOnlyAndShared()
    {
        Assert.True(FormatCultures.TryGet("de-DE", out CultureInfo? first));
        Assert.True(FormatCultures.TryGet("DE-DE", out CultureInfo? second));

        Assert.Same(first, second);
        Assert.True(first!.IsReadOnly);
    }

    [Fact]
    public void Cultures_DoNotDependOnTheGlobalizationDataOfThePlatform()
    {
        Assert.True(FormatCultures.TryGet("fr-FR", out CultureInfo? french));

        Assert.Equal(string.Empty, french!.Name);
        Assert.Equal(",", french.NumberFormat.NumberDecimalSeparator);
        Assert.Equal("vendredi", french.DateTimeFormat.GetDayName(DayOfWeek.Friday));
    }
}

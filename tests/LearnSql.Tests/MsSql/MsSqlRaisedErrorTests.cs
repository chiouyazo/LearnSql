using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Sql.Translation;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.MsSql;

public class MsSqlRaisedErrorTests
{
    private readonly MsSqlErrorTranslator _translator = new MsSqlErrorTranslator();

    public static TheoryData<MsSqlRaisedErrorKind, int, int, int> Definitions() =>
        new TheoryData<MsSqlRaisedErrorKind, int, int, int>
        {
            { MsSqlRaisedErrorKind.ConversionFailed, 245, 16, 1 },
            { MsSqlRaisedErrorKind.ConversionOverflow, 248, 16, 1 },
            { MsSqlRaisedErrorKind.DateConversionFailed, 241, 16, 1 },
            { MsSqlRaisedErrorKind.ErrorConvertingDataType, 8114, 16, 5 },
            { MsSqlRaisedErrorKind.ArithmeticOverflow, 8115, 16, 2 },
            { MsSqlRaisedErrorKind.NumericOverflow, 8115, 16, 8 },
            { MsSqlRaisedErrorKind.DivideByZero, 8134, 16, 1 },
            { MsSqlRaisedErrorKind.UnsupportedCulture, 9818, 16, 1 },
        };

    public static TheoryData<MsSqlRaisedErrorKind> Kinds()
    {
        TheoryData<MsSqlRaisedErrorKind> data = new TheoryData<MsSqlRaisedErrorKind>();
        foreach (MsSqlRaisedErrorKind kind in Enum.GetValues<MsSqlRaisedErrorKind>())
            data.Add(kind);
        return data;
    }

    private static MsSqlRaisedException Sample(MsSqlRaisedErrorKind kind)
    {
        return kind switch
        {
            MsSqlRaisedErrorKind.ConversionFailed => MsSqlRaisedErrors.ConversionFailed("x", "int"),
            MsSqlRaisedErrorKind.ConversionOverflow => MsSqlRaisedErrors.ConversionOverflow(
                "9",
                "int"
            ),
            MsSqlRaisedErrorKind.DateConversionFailed => MsSqlRaisedErrors.DateConversionFailed(),
            MsSqlRaisedErrorKind.ErrorConvertingDataType =>
                MsSqlRaisedErrors.ErrorConvertingDataType("float"),
            MsSqlRaisedErrorKind.ArithmeticOverflow => MsSqlRaisedErrors.ArithmeticOverflow("int"),
            MsSqlRaisedErrorKind.NumericOverflow => MsSqlRaisedErrors.NumericOverflow(),
            MsSqlRaisedErrorKind.DivideByZero => MsSqlRaisedErrors.DivideByZero(),
            MsSqlRaisedErrorKind.UnsupportedCulture => MsSqlRaisedErrors.UnsupportedCulture("zz"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static SqliteError Unknown(string message)
    {
        return new SqliteError(SqliteErrorType.Unknown, message, new Dictionary<string, string>());
    }

    [Theory]
    [MemberData(nameof(Definitions))]
    public void Catalog_DefinesNumberSeverityAndState(
        MsSqlRaisedErrorKind kind,
        int number,
        int severity,
        int state
    )
    {
        MsSqlErrorDefinition definition = MsSqlErrorCatalog.GetRaised(kind);

        Assert.Equal(number, definition.Number);
        Assert.Equal(severity, definition.Severity);
        Assert.Equal(state, definition.State);
    }

    [Fact]
    public void Catalog_DefinesEveryKind()
    {
        Assert.Equal(
            Enum.GetValues<MsSqlRaisedErrorKind>().Order(),
            MsSqlErrorCatalog.RaisedKinds.Order()
        );
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void Factory_SatisfiesTheTemplateOfItsKind(MsSqlRaisedErrorKind kind)
    {
        MsSqlRaisedException exception = Sample(kind);

        Assert.Equal(kind, exception.Kind);
        Assert.DoesNotContain("{", exception.Message);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void Exception_MessageRoundTripsThroughTheParser(MsSqlRaisedErrorKind kind)
    {
        MsSqlRaisedException exception = Sample(kind);

        Assert.True(
            MsSqlRaisedException.TryParse(
                exception.Message,
                out MsSqlRaisedErrorKind parsed,
                out string text
            )
        );
        Assert.Equal(kind, parsed);
        Assert.DoesNotContain("[mssql:", text);
        Assert.StartsWith(text, exception.Message);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void Translate_OfTheWrappedSqliteMessage_ReturnsTheCatalogError(
        MsSqlRaisedErrorKind kind
    )
    {
        MsSqlRaisedException exception = Sample(kind);
        SqliteError error = SqliteErrorClassifier.Classify(
            1,
            1,
            $"SQLite Error 1: '{exception.Message}'."
        );

        MsSqlError translated = _translator.Translate(error, 4);

        MsSqlErrorDefinition definition = MsSqlErrorCatalog.GetRaised(kind);
        Assert.Equal(definition.Number, translated.Number);
        Assert.Equal(definition.Severity, translated.Severity);
        Assert.Equal(definition.State, translated.State);
        Assert.Equal(4, translated.LineNumber);
        Assert.DoesNotContain("[mssql:", translated.Message);
    }

    [Fact]
    public void TryParse_OfPlainMessages_ReturnsFalse()
    {
        Assert.False(MsSqlRaisedException.TryParse("no such table: X", out _, out string text));
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryParse_OfUnknownKind_ReturnsFalse()
    {
        Assert.False(MsSqlRaisedException.TryParse("boom [mssql:NoSuchKind]", out _, out _));
        Assert.False(MsSqlRaisedException.TryParse("boom [mssql:42]", out _, out _));
    }

    [Fact]
    public void TryParse_KeepsMultilineText()
    {
        Assert.True(
            MsSqlRaisedException.TryParse(
                "SQLite Error 1: 'a\nb [mssql:DivideByZero]'.",
                out MsSqlRaisedErrorKind kind,
                out string text
            )
        );
        Assert.Equal(MsSqlRaisedErrorKind.DivideByZero, kind);
        Assert.Equal("a\nb", text);
    }

    [Fact]
    public void Translate_OfAMarkerOnAKnownSqliteType_IsIgnored()
    {
        SqliteError error = new SqliteError(
            SqliteErrorType.NoSuchTable,
            "x [mssql:DivideByZero]",
            new Dictionary<string, string> { ["Table"] = "T" }
        );

        MsSqlError translated = _translator.Translate(error);

        Assert.Equal(208, translated.Number);
    }

    [Fact]
    public void Translate_OfAnUnknownErrorWithoutMarker_UsesTheFallback()
    {
        MsSqlError translated = _translator.Translate(Unknown("some failure"));

        Assert.Equal(MsSqlErrorCatalog.FallbackNumber, translated.Number);
    }

    [Fact]
    public void Exception_ThrownByAFunction_ReachesTheTranslatorThroughSqlite()
    {
        using SqliteTestDatabase database = new SqliteTestDatabase();
        using SqliteCommand command = database.Connection.CreateCommand();
        command.CommandText = "SELECT TSQL_DIVISOR(0)";

        SqliteException exception = Assert.Throws<SqliteException>(() => command.ExecuteScalar());
        MsSqlError translated = _translator.CreateException(exception, 2).Error;

        Assert.Equal(8134, translated.Number);
        Assert.Equal("Divide by zero error encountered.", translated.Message);
        Assert.Equal("Msg 8134, Level 16, State 1, Line 2", translated.Header);
    }

    [Theory]
    [InlineData("9", "int", "an int")]
    [InlineData("9", "tinyint", "a tinyint")]
    [InlineData("9", "smallint", "a smallint")]
    public void ConversionOverflow_ChoosesTheArticleOfTheColumnType(
        string value,
        string type,
        string expectedColumn
    )
    {
        MsSqlRaisedException exception = MsSqlRaisedErrors.ConversionOverflow(value, type);

        Assert.Equal(expectedColumn, exception.Arguments[MsSqlRaisedErrors.ColumnTypeArgument]);
    }

    [Fact]
    public void Exception_WithMissingArguments_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new MsSqlRaisedException(MsSqlRaisedErrorKind.ConversionFailed)
        );
    }
}

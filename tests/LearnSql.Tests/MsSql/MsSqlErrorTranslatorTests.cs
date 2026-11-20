using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.MsSql;

public class MsSqlErrorTranslatorTests
{
    private readonly MsSqlErrorTranslator _translator = new MsSqlErrorTranslator();

    public static TheoryData<SqliteErrorType> MappedTypes()
    {
        TheoryData<SqliteErrorType> data = new TheoryData<SqliteErrorType>();
        foreach (SqliteErrorType type in MsSqlErrorCatalog.MappedTypes.Order())
            data.Add(type);
        return data;
    }

    public static TheoryData<SqliteErrorType> AllTypes()
    {
        TheoryData<SqliteErrorType> data = new TheoryData<SqliteErrorType>();
        foreach (SqliteErrorType type in Enum.GetValues<SqliteErrorType>())
            data.Add(type);
        return data;
    }

    private static SqliteError ErrorWithAllArguments(SqliteErrorType type)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        foreach (string name in SqliteErrorArguments.GetNames(type))
            arguments[name] = name.Contains("Columns") ? "colA, T.colB" : "value" + name;
        if (!arguments.ContainsKey("Table"))
            arguments["Table"] = "Orders";
        return new SqliteError(type, "original message", arguments);
    }

    private static SqliteError ErrorWithoutArguments(SqliteErrorType type)
    {
        return new SqliteError(type, "original message", new Dictionary<string, string>());
    }

    [Theory]
    [MemberData(nameof(AllTypes))]
    public void Translate_AnyTypeWithAllArguments_LeavesNoBraces(SqliteErrorType type)
    {
        MsSqlError error = _translator.Translate(ErrorWithAllArguments(type));

        Assert.DoesNotContain('{', error.Message);
        Assert.DoesNotContain('}', error.Message);
    }

    [Theory]
    [MemberData(nameof(AllTypes))]
    public void Translate_AnyTypeWithEmptyArguments_LeavesNoBraces(SqliteErrorType type)
    {
        MsSqlError error = _translator.Translate(ErrorWithoutArguments(type));

        Assert.DoesNotContain('{', error.Message);
        Assert.DoesNotContain('}', error.Message);
    }

    [Theory]
    [MemberData(nameof(MappedTypes))]
    public void Translate_MappedTypeWithAllArguments_UsesTheCatalogNumber(SqliteErrorType type)
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);

        MsSqlError error = _translator.Translate(ErrorWithAllArguments(type));

        Assert.Equal(definition!.Number, error.Number);
        Assert.Equal(definition.Severity, error.Severity);
        Assert.Equal(definition.State, error.State);
    }

    [Theory]
    [MemberData(nameof(MappedTypes))]
    public void Translate_MappedTypeWithoutArguments_NeverLeavesAPlaceholderBehind(
        SqliteErrorType type
    )
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);
        bool needsArguments = MessageTemplate.GetPlaceholders(definition!.Template).Count > 0;

        MsSqlError error = _translator.Translate(ErrorWithoutArguments(type));

        if (needsArguments)
            Assert.Equal(MsSqlErrorCatalog.FallbackNumber, error.Number);
        else
            Assert.Equal(definition.Number, error.Number);
    }

    [Theory]
    [MemberData(nameof(AllTypes))]
    public void Translate_AnyType_PassesTheLineNumberThrough(SqliteErrorType type)
    {
        MsSqlError error = _translator.Translate(ErrorWithAllArguments(type), 12);

        Assert.Equal(12, error.LineNumber);
    }

    [Fact]
    public void Translate_UnknownType_ProducesTheFallbackWithTheOriginalMessage()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.Unknown,
            "disk I/O error",
            new Dictionary<string, string>()
        );

        MsSqlError error = _translator.Translate(original);

        Assert.Equal(new MsSqlError(50000, 16, 1, "Disk I/O error."), error);
    }

    [Fact]
    public void Translate_UnmappedKnownType_ProducesTheFallback()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.CorruptDatabase,
            "corrupt database",
            new Dictionary<string, string>()
        );

        MsSqlError error = _translator.Translate(original);

        Assert.Equal(50000, error.Number);
        Assert.Equal("Corrupt database.", error.Message);
    }

    [Fact]
    public void Translate_MappedTypeWithEmptyArgumentValue_FallsBackInsteadOfRenderingAGap()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.NoSuchTable,
            "no such table: ",
            new Dictionary<string, string> { ["Table"] = "" }
        );

        MsSqlError error = _translator.Translate(original);

        Assert.Equal(50000, error.Number);
    }

    [Fact]
    public void Translate_NullError_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _translator.Translate(null!));
    }

    [Fact]
    public void Translate_PrimaryKey_DerivesTheConstraintNameFromTheTable()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.PrimaryKeyConstraintFailed,
            "m",
            new Dictionary<string, string> { ["Table"] = "Orders", ["Columns"] = "Orders.Id" }
        );

        MsSqlError error = _translator.Translate(original);

        Assert.Equal(
            "Violation of PRIMARY KEY constraint 'PK_Orders'. Cannot insert duplicate key in object 'dbo.Orders'.",
            error.Message
        );
    }

    [Fact]
    public void Translate_Unique_DerivesTheConstraintNameFromTableAndColumns()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.UniqueConstraintFailed,
            "m",
            new Dictionary<string, string> { ["Table"] = "Orders", ["Columns"] = "A, Orders.B" }
        );

        MsSqlError error = _translator.Translate(original);

        Assert.Equal(
            "Violation of UNIQUE KEY constraint 'UQ_Orders_A_B'. Cannot insert duplicate key in object 'dbo.Orders'.",
            error.Message
        );
    }

    [Fact]
    public void Translate_UniqueWithoutColumns_FallsBack()
    {
        SqliteError original = new SqliteError(
            SqliteErrorType.UniqueConstraintFailed,
            "m",
            new Dictionary<string, string> { ["Table"] = "Orders" }
        );

        Assert.Equal(50000, _translator.Translate(original).Number);
    }

    [Fact]
    public void Translate_DoesNotMutateTheArgumentsOfTheInputError()
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>
        {
            ["Table"] = "Orders",
            ["Columns"] = "Id",
        };
        SqliteError original = new SqliteError(
            SqliteErrorType.PrimaryKeyConstraintFailed,
            "m",
            arguments
        );

        _translator.Translate(original);

        Assert.Equal(2, arguments.Count);
        Assert.False(arguments.ContainsKey(MsSqlErrorTranslator.ConstraintArgument));
    }

    [Fact]
    public void GetDerivedArgumentNames_ForConstraintTypes_ListsTheConstraintArgument()
    {
        Assert.Equal(
            ["Constraint"],
            MsSqlErrorTranslator.GetDerivedArgumentNames(SqliteErrorType.UniqueConstraintFailed)
        );
        Assert.Equal(
            ["Constraint"],
            MsSqlErrorTranslator.GetDerivedArgumentNames(SqliteErrorType.PrimaryKeyConstraintFailed)
        );
        Assert.Empty(MsSqlErrorTranslator.GetDerivedArgumentNames(SqliteErrorType.NoSuchTable));
    }

    [Fact]
    public void Language_IsMsSql()
    {
        Assert.Equal(Language.MsSql, _translator.Language);
    }

    public static TheoryData<string, string, string> RealFailures() =>
        new TheoryData<string, string, string>
        {
            {
                "SELECT * FROM Missing",
                "Msg 208, Level 16, State 1",
                "Invalid object name 'Missing'."
            },
            {
                "SELECT Missing FROM Parent",
                "Msg 207, Level 16, State 1",
                "Invalid column name 'Missing'."
            },
            { "SELEC 1", "Msg 102, Level 15, State 1", "Incorrect syntax near 'SELEC'." },
            {
                "SELECT NoSuchFunction(1)",
                "Msg 195, Level 15, State 10",
                "'NoSuchFunction' is not a recognized built-in function name."
            },
            {
                "CREATE TABLE Parent (Id INTEGER)",
                "Msg 2714, Level 16, State 6",
                "There is already an object named 'Parent' in the database."
            },
            {
                "INSERT INTO Parent VALUES (1, 'B')",
                "Msg 2627, Level 14, State 1",
                "Violation of PRIMARY KEY constraint 'PK_Parent'. Cannot insert duplicate key in object 'dbo.Parent'."
            },
            {
                "INSERT INTO Parent VALUES (2, 'A')",
                "Msg 2627, Level 14, State 1",
                "Violation of UNIQUE KEY constraint 'UQ_Parent_Code'. Cannot insert duplicate key in object 'dbo.Parent'."
            },
            {
                "INSERT INTO Child (Id, Qty) VALUES (1, 1)",
                "Msg 515, Level 16, State 2",
                "Cannot insert the value NULL into column 'ParentId', table 'dbo.Child'; column does not allow nulls. INSERT fails."
            },
            {
                "INSERT INTO Child VALUES (1, 99, 1)",
                "Msg 547, Level 16, State 0",
                "The statement conflicted with the FOREIGN KEY constraint."
            },
            {
                "INSERT INTO Parent (Id, Missing) VALUES (5, 'X')",
                "Msg 207, Level 16, State 1",
                "Invalid column name 'Missing'."
            },
            {
                "INSERT INTO Parent VALUES (5, 'X', 'extra')",
                "Msg 213, Level 16, State 1",
                "Column name or number of supplied values does not match table definition."
            },
            {
                "DROP INDEX Missing",
                "Msg 3701, Level 11, State 5",
                "Cannot drop the index 'Missing', because it does not exist or you do not have permission."
            },
            { "SELECT *", "Msg 1038, Level 15, State 1", "Must specify table to select from." },
        };

    [Theory]
    [MemberData(nameof(RealFailures))]
    public void CreateException_RealSqliteFailure_ProducesTheExpectedMsSqlError(
        string failingSql,
        string expectedHeader,
        string expectedMessage
    )
    {
        SqliteException exception = SqliteFailure.Capture(SqliteFailure.ShopSetup, failingSql);

        TranslatedException translated = _translator.CreateException(exception);

        Assert.Equal(expectedHeader, translated.Error.Header);
        Assert.Equal(expectedMessage, translated.Error.Message);
    }

    [Fact]
    public void CreateException_RealCheckViolation_QuotesTheConstraintInTheMessage()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child VALUES (1, 1, 0)"
        );

        TranslatedException translated = _translator.CreateException(exception);

        Assert.Equal(547, translated.Error.Number);
        Assert.StartsWith(
            "The statement conflicted with the CHECK constraint \"",
            translated.Error.Message
        );
        Assert.Contains("Qty", translated.Error.Message);
    }

    [Fact]
    public void CreateException_KeepsTheOriginalExceptionSqliteErrorAndLineNumber()
    {
        SqliteException exception = SqliteFailure.Capture("", "SELECT * FROM Missing");

        TranslatedException translated = _translator.CreateException(exception, 4);

        Assert.Same(exception, translated.InnerException);
        Assert.Equal(SqliteErrorType.NoSuchTable, translated.SqliteError.Type);
        Assert.Equal(4, translated.Error.LineNumber);
        Assert.Equal(translated.Error.ToString(), translated.Message);
    }

    [Fact]
    public void CreateException_NullException_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _translator.CreateException(null!));
    }
}

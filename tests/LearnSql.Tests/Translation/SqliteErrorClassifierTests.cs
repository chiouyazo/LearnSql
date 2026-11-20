using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.Translation;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Translation;

public class SqliteErrorClassifierTests
{
    public static TheoryData<string, string, int, SqliteErrorType> RealConstraintFailures() =>
        new TheoryData<string, string, int, SqliteErrorType>
        {
            {
                "check",
                "INSERT INTO Child VALUES (1, 1, 0)",
                275,
                SqliteErrorType.CheckConstraintFailed
            },
            {
                "foreign key",
                "INSERT INTO Child VALUES (1, 99, 1)",
                787,
                SqliteErrorType.ForeignKeyConstraintFailed
            },
            {
                "not null",
                "INSERT INTO Child (Id, Qty) VALUES (1, 1)",
                1299,
                SqliteErrorType.NotNullConstraintFailed
            },
            {
                "primary key",
                "INSERT INTO Parent VALUES (1, 'B')",
                1555,
                SqliteErrorType.PrimaryKeyConstraintFailed
            },
            {
                "unique",
                "INSERT INTO Parent VALUES (2, 'A')",
                2067,
                SqliteErrorType.UniqueConstraintFailed
            },
        };

    [Theory]
    [MemberData(nameof(RealConstraintFailures))]
    public void Classify_RealSqliteConstraintFailure_ReportsTheExpectedExtendedCodeAndType(
        string caseName,
        string failingSql,
        int expectedExtendedCode,
        SqliteErrorType expectedType
    )
    {
        SqliteException exception = SqliteFailure.Capture(SqliteFailure.ShopSetup, failingSql);

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Equal(SqliteErrorClassifier.ConstraintResultCode, exception.SqliteErrorCode);
        Assert.Equal(expectedExtendedCode, exception.SqliteExtendedErrorCode);
        Assert.True(error.Type == expectedType, $"Unexpected type for case '{caseName}'.");
    }

    [Fact]
    public void Classify_RealPrimaryKeyViolation_IsNotReportedAsUniqueDespiteTheUniqueMessage()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Parent VALUES (1, 'B')"
        );

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.StartsWith("SQLite Error 19", exception.Message);
        Assert.Equal(SqliteErrorType.PrimaryKeyConstraintFailed, error.Type);
        Assert.Equal("Parent", error.Arguments["Table"]);
        Assert.Equal("Id", error.Arguments["Columns"]);
    }

    [Fact]
    public void Classify_RealUniqueViolation_ExtractsTableAndColumns()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Parent VALUES (2, 'A')"
        );

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Equal("Parent", error.Arguments["Table"]);
        Assert.Equal("Code", error.Arguments["Columns"]);
    }

    [Fact]
    public void Classify_RealNotNullViolation_ExtractsTableAndColumn()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child (Id, Qty) VALUES (1, 1)"
        );

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Equal("Child", error.Arguments["Table"]);
        Assert.Equal("ParentId", error.Arguments["Column"]);
    }

    [Fact]
    public void Classify_RealCheckViolation_ExtractsTheConstraintText()
    {
        SqliteException exception = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child VALUES (1, 1, 0)"
        );

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Contains("Qty", error.Arguments["Constraint"]);
    }

    [Fact]
    public void Classify_RealCompositePrimaryKeyViolation_KeepsAllColumns()
    {
        SqliteException exception = SqliteFailure.Capture(
            "CREATE TABLE Link (A INTEGER, B INTEGER, PRIMARY KEY (A, B)); INSERT INTO Link VALUES (1, 2);",
            "INSERT INTO Link VALUES (1, 2)"
        );

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Equal(SqliteErrorType.PrimaryKeyConstraintFailed, error.Type);
        Assert.Equal("Link", error.Arguments["Table"]);
        Assert.Equal("A, Link.B", error.Arguments["Columns"]);
    }

    [Theory]
    [InlineData("SELECT * FROM Missing", 1, SqliteErrorType.NoSuchTable)]
    [InlineData("SELECT Missing FROM Parent", 1, SqliteErrorType.NoSuchColumn)]
    [InlineData("SELEC 1", 1, SqliteErrorType.NearSyntaxError)]
    [InlineData("SELECT NoSuchFunction(1)", 1, SqliteErrorType.NoSuchFunction)]
    [InlineData("CREATE TABLE Parent (Id INTEGER)", 1, SqliteErrorType.AlreadyExists)]
    public void Classify_RealNonConstraintFailure_UsesTheMessageBasedType(
        string failingSql,
        int expectedResultCode,
        SqliteErrorType expectedType
    )
    {
        SqliteException exception = SqliteFailure.Capture(SqliteFailure.ShopSetup, failingSql);

        SqliteError error = SqliteErrorClassifier.Classify(exception);

        Assert.Equal(expectedResultCode, exception.SqliteErrorCode);
        Assert.Equal(expectedType, error.Type);
    }

    [Theory]
    [InlineData(275, SqliteErrorType.CheckConstraintFailed)]
    [InlineData(787, SqliteErrorType.ForeignKeyConstraintFailed)]
    [InlineData(1299, SqliteErrorType.NotNullConstraintFailed)]
    [InlineData(1555, SqliteErrorType.PrimaryKeyConstraintFailed)]
    [InlineData(2067, SqliteErrorType.UniqueConstraintFailed)]
    public void Classify_ConstraintCodeWithUnrelatedMessage_TrustsTheExtendedCode(
        int extendedCode,
        SqliteErrorType expectedType
    )
    {
        SqliteError error = SqliteErrorClassifier.Classify(19, extendedCode, "something odd");

        Assert.Equal(expectedType, error.Type);
        Assert.Empty(error.Arguments);
        Assert.Equal("something odd", error.Message);
    }

    [Fact]
    public void Classify_ConstraintCodeWithMatchingMessage_KeepsTheMappedArguments()
    {
        SqliteError error = SqliteErrorClassifier.Classify(
            19,
            2067,
            "UNIQUE constraint failed: Parent.Code"
        );

        Assert.Equal(SqliteErrorType.UniqueConstraintFailed, error.Type);
        Assert.Equal("Parent", error.Arguments["Table"]);
        Assert.Equal("Code", error.Arguments["Columns"]);
    }

    [Fact]
    public void Classify_PrimaryKeyCodeWithUniqueMessage_KeepsTheArgumentsAndChangesTheType()
    {
        SqliteError error = SqliteErrorClassifier.Classify(
            19,
            1555,
            "UNIQUE constraint failed: Parent.Id"
        );

        Assert.Equal(SqliteErrorType.PrimaryKeyConstraintFailed, error.Type);
        Assert.Equal("Parent", error.Arguments["Table"]);
        Assert.Equal("Id", error.Arguments["Columns"]);
    }

    [Fact]
    public void Classify_ConstraintCodeWithUnknownExtendedCode_FallsBackToTheMessage()
    {
        SqliteError error = SqliteErrorClassifier.Classify(
            19,
            1811,
            "NOT NULL constraint failed: t.c"
        );

        Assert.Equal(SqliteErrorType.NotNullConstraintFailed, error.Type);
    }

    [Fact]
    public void Classify_NonConstraintCode_IgnoresTheExtendedCode()
    {
        SqliteError error = SqliteErrorClassifier.Classify(1, 275, "no such table: Missing");

        Assert.Equal(SqliteErrorType.NoSuchTable, error.Type);
        Assert.Equal("Missing", error.Arguments["Table"]);
    }

    [Fact]
    public void Classify_WrappedMessage_IsUnwrapped()
    {
        SqliteError error = SqliteErrorClassifier.Classify(
            1,
            1,
            "SQLite Error 1: 'no such table: Missing'."
        );

        Assert.Equal("no such table: Missing", error.Message);
    }

    [Fact]
    public void Classify_NullException_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SqliteErrorClassifier.Classify(null!));
    }

    [Fact]
    public void Classify_NullMessage_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SqliteErrorClassifier.Classify(1, 1, null!));
    }

    [Fact]
    public void Constants_MatchTheDocumentedSqliteExtendedResultCodes()
    {
        Assert.Equal(19, SqliteErrorClassifier.ConstraintResultCode);
        Assert.Equal(275, SqliteErrorClassifier.ConstraintCheckExtendedCode);
        Assert.Equal(787, SqliteErrorClassifier.ConstraintForeignKeyExtendedCode);
        Assert.Equal(1299, SqliteErrorClassifier.ConstraintNotNullExtendedCode);
        Assert.Equal(1555, SqliteErrorClassifier.ConstraintPrimaryKeyExtendedCode);
        Assert.Equal(2067, SqliteErrorClassifier.ConstraintUniqueExtendedCode);
    }
}

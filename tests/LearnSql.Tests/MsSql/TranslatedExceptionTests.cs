using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Tests.MsSql;

public class TranslatedExceptionTests
{
    private static readonly SqliteError Sqlite = new SqliteError(
        SqliteErrorType.NoSuchTable,
        "no such table: T",
        new Dictionary<string, string> { ["Table"] = "T" }
    );

    private static readonly MsSqlError MsSql = new MsSqlError(
        208,
        16,
        1,
        "Invalid object name 'T'.",
        2
    );

    [Fact]
    public void Constructor_ExposesSqliteErrorAndErrorByReference()
    {
        TranslatedException exception = new TranslatedException(Sqlite, MsSql);

        Assert.Same(Sqlite, exception.SqliteError);
        Assert.Same(MsSql, exception.Error);
    }

    [Fact]
    public void Constructor_ExposesTheInnerExceptionByReference()
    {
        InvalidOperationException inner = new InvalidOperationException("inner");

        TranslatedException exception = new TranslatedException(Sqlite, MsSql, inner);

        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithoutInnerException_HasNone()
    {
        TranslatedException exception = new TranslatedException(Sqlite, MsSql);

        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Message_IsTheMsSqlErrorText()
    {
        TranslatedException exception = new TranslatedException(Sqlite, MsSql);

        Assert.Equal(
            "Msg 208, Level 16, State 1, Line 2\nInvalid object name 'T'.",
            exception.Message
        );
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new TranslatedException(null!, MsSql));
        Assert.Throws<ArgumentNullException>(() => new TranslatedException(Sqlite, null!));
    }
}

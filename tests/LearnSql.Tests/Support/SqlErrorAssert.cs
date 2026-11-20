using LearnSql.Sql.MsSql;

namespace LearnSql.Tests.Support;

public static class SqlErrorAssert
{
    public static void Raised(
        ScriptOutcome outcome,
        int number,
        int severity,
        int state,
        string message
    )
    {
        Assert.True(outcome.TranslationSucceeded, outcome.Describe());
        Assert.NotNull(outcome.Error);
        MsSqlError error = outcome.Error.Error;
        Assert.Equal(number, error.Number);
        Assert.Equal(severity, error.Severity);
        Assert.Equal(state, error.State);
        Assert.Equal(message, error.Message);
    }
}

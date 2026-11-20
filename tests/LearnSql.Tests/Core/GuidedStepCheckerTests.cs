using LearnSql.Core.Services.Guided;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Core;

public sealed class GuidedStepCheckerTests
{
    private static GuidedStep Step(
        GuidedStepKind kind = GuidedStepKind.ReadOnly,
        int? rows = null,
        int? affected = null,
        bool expectsError = false
    )
    {
        return new GuidedStep
        {
            Id = "s1",
            Title = "Titel",
            Sql = "SELECT 1;",
            Kind = kind,
            QueryExplanation = new QueryExplanation { Summary = "s", Parts = [] },
            ResultExplanation = "r",
            DatabaseEffect = "e",
            ExpectedResultRows = rows,
            ExpectedRowsAffected = affected,
            ExpectsError = expectsError,
        };
    }

    private static ExecutionResult Rows(int count, bool rolledBack = false)
    {
        object?[][] data = [.. Enumerable.Range(0, count).Select((int i) => new object?[] { i })];
        ResultSet set = new ResultSet([], data, false);
        StatementResult statement = new StatementResult(
            0,
            0,
            StatementKind.Select,
            default,
            [set],
            null,
            TimeSpan.Zero,
            []
        );
        return new ExecutionResult([statement], [], null, TimeSpan.Zero, rolledBack);
    }

    private static ExecutionResult Affected(int count, bool rolledBack = false)
    {
        StatementResult statement = new StatementResult(
            0,
            0,
            StatementKind.Insert,
            default,
            [],
            count,
            TimeSpan.Zero,
            []
        );
        return new ExecutionResult([statement], [], null, TimeSpan.Zero, rolledBack);
    }

    private static ExecutionResult Failed(ExecutionErrorKind kind)
    {
        return new ExecutionResult(
            [],
            [],
            new ExecutionError(kind, "Fehlertext"),
            TimeSpan.Zero,
            false
        );
    }

    [Fact]
    public void Check_RowCountEqual_Matched()
    {
        GuidedCheck check = GuidedStepChecker.Check(Step(rows: 3), Rows(3));

        Assert.Equal(GuidedCheckKind.Matched, check.Kind);
        Assert.True(check.IsExpected);
        Assert.True(check.CountsAsDone);
    }

    [Fact]
    public void Check_RowCountDiffers_ReportsExpectedAndActual()
    {
        GuidedCheck check = GuidedStepChecker.Check(Step(rows: 3), Rows(5));

        Assert.Equal(GuidedCheckKind.RowCountDiffers, check.Kind);
        Assert.Equal(3, check.Expected);
        Assert.Equal(5, check.Actual);
        Assert.False(check.IsExpected);
        Assert.True(check.CountsAsDone);
    }

    [Fact]
    public void Check_NoExpectation_Matched()
    {
        GuidedCheck check = GuidedStepChecker.Check(Step(), Rows(7));

        Assert.Equal(GuidedCheckKind.Matched, check.Kind);
    }

    [Fact]
    public void Check_RowsAffectedDiffers_Reported()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, affected: 3),
            Affected(1)
        );

        Assert.Equal(GuidedCheckKind.RowsAffectedDiffers, check.Kind);
        Assert.Equal(3, check.Expected);
        Assert.Equal(1, check.Actual);
    }

    [Fact]
    public void Check_RowsAffectedEqual_Matched()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, affected: 2),
            Affected(2)
        );

        Assert.Equal(GuidedCheckKind.Matched, check.Kind);
    }

    [Fact]
    public void Check_ExpectedErrorOccurred_IsLearningEffect()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, expectsError: true),
            Failed(ExecutionErrorKind.Sql)
        );

        Assert.Equal(GuidedCheckKind.ExpectedErrorOccurred, check.Kind);
        Assert.Equal("Fehlertext", check.ErrorText);
        Assert.True(check.IsExpected);
        Assert.True(check.CountsAsDone);
    }

    [Fact]
    public void Check_ExpectedErrorMissing_Deviation()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, expectsError: true),
            Affected(1)
        );

        Assert.Equal(GuidedCheckKind.ExpectedErrorMissing, check.Kind);
        Assert.False(check.IsExpected);
    }

    [Fact]
    public void Check_UnexpectedError_NotDone()
    {
        GuidedCheck check = GuidedStepChecker.Check(Step(), Failed(ExecutionErrorKind.Sql));

        Assert.Equal(GuidedCheckKind.UnexpectedError, check.Kind);
        Assert.Equal("Fehlertext", check.ErrorText);
        Assert.False(check.CountsAsDone);
    }

    [Theory]
    [InlineData(ExecutionErrorKind.Timeout)]
    [InlineData(ExecutionErrorKind.Internal)]
    public void Check_TimeoutOrInternal_NeverCountsAsExpectedError(ExecutionErrorKind kind)
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, expectsError: true),
            Failed(kind)
        );

        Assert.Equal(GuidedCheckKind.UnexpectedError, check.Kind);
    }

    [Fact]
    public void Check_Cancelled_Interrupted()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(expectsError: true),
            Failed(ExecutionErrorKind.Cancelled)
        );

        Assert.Equal(GuidedCheckKind.Interrupted, check.Kind);
        Assert.False(check.CountsAsDone);
    }

    [Fact]
    public void Check_RolledBackChange_NotDone()
    {
        GuidedCheck check = GuidedStepChecker.Check(
            Step(GuidedStepKind.ChangesData, affected: 1),
            Affected(1, rolledBack: true)
        );

        Assert.Equal(GuidedCheckKind.RolledBack, check.Kind);
        Assert.False(check.CountsAsDone);
    }

    [Fact]
    public void Check_RolledBackReadOnly_StillCompared()
    {
        GuidedCheck check = GuidedStepChecker.Check(Step(rows: 2), Rows(2, rolledBack: true));

        Assert.Equal(GuidedCheckKind.Matched, check.Kind);
    }
}

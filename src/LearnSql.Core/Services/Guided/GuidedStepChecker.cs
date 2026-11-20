namespace LearnSql.Core.Services.Guided;

public static class GuidedStepChecker
{
    public static GuidedCheck Check(GuidedStep step, ExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(result);

        ExecutionError? error = result.Error;
        if (error is not null)
        {
            return CheckError(step, error);
        }

        if (step.ExpectsError)
        {
            return new GuidedCheck(GuidedCheckKind.ExpectedErrorMissing);
        }

        if (result.RolledBack && step.Kind == GuidedStepKind.ChangesData)
        {
            return new GuidedCheck(GuidedCheckKind.RolledBack);
        }

        int rows = result.ResultSets.Sum((ResultSet set) => set.Rows.Count);
        if (step.ExpectedResultRows is int expectedRows && expectedRows != rows)
        {
            return new GuidedCheck(GuidedCheckKind.RowCountDiffers, expectedRows, rows);
        }

        int affected = result.Statements.Sum(
            (StatementResult statement) => statement.RowsAffected ?? 0
        );
        if (step.ExpectedRowsAffected is int expectedAffected && expectedAffected != affected)
        {
            return new GuidedCheck(GuidedCheckKind.RowsAffectedDiffers, expectedAffected, affected);
        }

        return new GuidedCheck(GuidedCheckKind.Matched);
    }

    private static GuidedCheck CheckError(GuidedStep step, ExecutionError error)
    {
        if (error.Kind == ExecutionErrorKind.Cancelled)
        {
            return new GuidedCheck(GuidedCheckKind.Interrupted);
        }

        bool statementError =
            error.Kind
            is ExecutionErrorKind.Sql
                or ExecutionErrorKind.Diagnostics
                or ExecutionErrorKind.Rejected
                or ExecutionErrorKind.LimitExceeded;
        if (step.ExpectsError && statementError)
        {
            return new GuidedCheck(GuidedCheckKind.ExpectedErrorOccurred, ErrorText: error.Display);
        }

        return new GuidedCheck(GuidedCheckKind.UnexpectedError, ErrorText: error.Display);
    }
}

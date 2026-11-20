namespace LearnSql.Core.Services.Guided;

public enum GuidedCheckKind
{
    Matched,
    ExpectedErrorOccurred,
    RowCountDiffers,
    RowsAffectedDiffers,
    ExpectedErrorMissing,
    UnexpectedError,
    RolledBack,
    Interrupted,
}

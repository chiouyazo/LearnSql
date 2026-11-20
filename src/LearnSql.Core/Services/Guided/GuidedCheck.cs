namespace LearnSql.Core.Services.Guided;

public sealed record GuidedCheck(
    GuidedCheckKind Kind,
    int? Expected = null,
    int? Actual = null,
    string? ErrorText = null
)
{
    public bool IsExpected =>
        Kind is GuidedCheckKind.Matched or GuidedCheckKind.ExpectedErrorOccurred;

    public bool CountsAsDone =>
        Kind
            is GuidedCheckKind.Matched
                or GuidedCheckKind.ExpectedErrorOccurred
                or GuidedCheckKind.RowCountDiffers
                or GuidedCheckKind.RowsAffectedDiffers
                or GuidedCheckKind.ExpectedErrorMissing;
}

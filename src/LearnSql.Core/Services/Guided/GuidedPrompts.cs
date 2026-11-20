namespace LearnSql.Core.Services.Guided;

public sealed class GuidedPrompts
{
    private readonly IDialogService _dialogs;
    private readonly ISnackbar _snackbar;
    private readonly IStringLocalizer<GuidedStrings> _l;

    public GuidedPrompts(
        IDialogService dialogs,
        ISnackbar snackbar,
        IStringLocalizer<GuidedStrings> localizer
    )
    {
        _dialogs = dialogs;
        _snackbar = snackbar;
        _l = localizer;
    }

    public async Task<bool> ConfirmResetAsync()
    {
        bool? confirmed = await _dialogs.ShowMessageBoxAsync(
            _l["ResetDialogTitle"],
            _l["ResetDialogMessage"],
            yesText: _l["ResetDialogConfirm"],
            cancelText: _l["Cancel"]
        );
        return confirmed == true;
    }

    public void Report(GuidedActionResult result)
    {
        LocalizedString? message = result switch
        {
            GuidedActionResult.Busy => _l["NoticeBusy"],
            GuidedActionResult.NotReady => _l["NoticeNotReady"],
            GuidedActionResult.EditorUnavailable => _l["NoticeEditorUnavailable"],
            _ => null,
        };
        if (message is not null)
        {
            _snackbar.Add(message.Value, Severity.Warning);
        }
    }
}

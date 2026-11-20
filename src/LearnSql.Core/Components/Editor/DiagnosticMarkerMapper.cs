namespace LearnSql.Core.Components.Editor;

public static class DiagnosticMarkerMapper
{
    public static IReadOnlyList<EditorMarker> ToMarkers(
        IEnumerable<DialectDiagnostic> diagnostics,
        SourceOrigin origin
    )
    {
        return
        [
            .. diagnostics.Select(
                (DialectDiagnostic diagnostic) =>
                    new EditorMarker(
                        origin.ToEditorRange(diagnostic.Span),
                        string.IsNullOrWhiteSpace(diagnostic.Hint)
                            ? diagnostic.Message
                            : $"{diagnostic.Message}\n{diagnostic.Hint}",
                        ToSeverity(diagnostic.Severity),
                        diagnostic.Code
                    )
            ),
        ];
    }

    private static EditorMarkerSeverity ToSeverity(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Error => EditorMarkerSeverity.Error,
            DiagnosticSeverity.Warning => EditorMarkerSeverity.Warning,
            _ => EditorMarkerSeverity.Info,
        };
    }
}

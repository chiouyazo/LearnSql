namespace LearnSql.Core.Components.Editor;

public enum EditorMarkerSeverity
{
    Hint,
    Info,
    Warning,
    Error,
}

public sealed record EditorMarker(
    EditorRange Range,
    string Message,
    EditorMarkerSeverity Severity,
    string? Code = null
);

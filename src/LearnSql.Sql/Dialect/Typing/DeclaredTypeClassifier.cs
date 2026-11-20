namespace LearnSql.Sql.Dialect.Typing;

public static class DeclaredTypeClassifier
{
    private static readonly string[] TextMarkers = ["CHAR", "CLOB", "TEXT", "UNIQUEIDENTIFIER"];
    private static readonly string[] DateTimeMarkers = ["DATE", "TIME"];
    private static readonly string[] BinaryMarkers = ["BLOB", "BINARY", "IMAGE"];
    private static readonly string[] NumericMarkers =
    [
        "INT",
        "REAL",
        "FLOA",
        "DOUB",
        "DEC",
        "NUM",
        "MONEY",
        "BIT",
    ];

    public static ValueKind Classify(string? declaredType)
    {
        if (string.IsNullOrWhiteSpace(declaredType))
            return ValueKind.Unknown;

        string upper = declaredType.ToUpperInvariant();
        if (ContainsAny(upper, TextMarkers))
            return ValueKind.Text;
        if (ContainsAny(upper, DateTimeMarkers))
            return ValueKind.DateTime;
        if (ContainsAny(upper, BinaryMarkers))
            return ValueKind.Binary;
        if (ContainsAny(upper, NumericMarkers))
            return ValueKind.Numeric;
        return ValueKind.Unknown;
    }

    private static bool ContainsAny(string value, string[] markers)
    {
        foreach (string marker in markers)
        {
            if (value.Contains(marker, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}

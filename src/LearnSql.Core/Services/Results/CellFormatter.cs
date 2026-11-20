namespace LearnSql.Core.Services.Results;

public static class CellFormatter
{
    public const string NullText = "NULL";

    public static string Format(object? value)
    {
        return value switch
        {
            null => NullText,
            string text => text,
            long number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString("R", CultureInfo.InvariantCulture),
            byte[] bytes => "0x" + Convert.ToHexString(bytes),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    public static bool IsNumeric(object? value)
    {
        return value is long or double or decimal or int;
    }
}

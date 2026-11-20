using System.Globalization;
using System.Text;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Dialect.Functions;

public static class SqlValue
{
    public static bool IsNull(object? value)
    {
        return value is null || value is DBNull;
    }

    public static string ToText(object value)
    {
        switch (value)
        {
            case string text:
                return text;
            case long integer:
                return integer.ToString(CultureInfo.InvariantCulture);
            case double number:
                return number.ToString("R", CultureInfo.InvariantCulture);
            case byte[] bytes:
                return ToHex(bytes);
            default:
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    public static long ToLong(object value, string targetTypeName = "int")
    {
        switch (value)
        {
            case long integer:
                return integer;
            case double number:
                return checked((long)Math.Truncate(number));
            case string text:
                if (
                    long.TryParse(
                        text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out long parsed
                    )
                )
                    return parsed;
                if (
                    double.TryParse(
                        text.Trim(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double fraction
                    )
                )
                    return checked((long)Math.Truncate(fraction));
                throw ConversionFailed(text, targetTypeName);
            default:
                throw ConversionFailed(ToText(value), targetTypeName);
        }
    }

    public static double ToDouble(object value, string targetTypeName = "float")
    {
        switch (value)
        {
            case long integer:
                return integer;
            case double number:
                return number;
            case string text:
                if (
                    double.TryParse(
                        text.Trim(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double parsed
                    )
                )
                    return parsed;
                throw ConversionFailed(text, targetTypeName);
            default:
                throw ConversionFailed(ToText(value), targetTypeName);
        }
    }

    public static MsSqlRaisedException ConversionFailed(string value, string targetTypeName)
    {
        return MsSqlRaisedErrors.ConversionFailed(value, targetTypeName);
    }

    private static string ToHex(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder("0x");
        foreach (byte item in bytes)
            builder.Append(item.ToString("X2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }
}

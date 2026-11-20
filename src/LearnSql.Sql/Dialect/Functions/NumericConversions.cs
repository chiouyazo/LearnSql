using System.Globalization;
using System.Text.RegularExpressions;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Dialect.Functions;

public static partial class NumericConversions
{
    private const double LongRangeLimit = 9.2233720368547758E18;
    private const decimal DecimalRangeLimit = 7.9E28m;
    private const int MaxDecimalScale = 28;

    [GeneratedRegex(@"^[+-]?[0-9]+$")]
    private static partial Regex IntegerSyntax();

    [GeneratedRegex(@"^[+-]?([0-9]+\.?[0-9]*|\.[0-9]+)$")]
    private static partial Regex DecimalSyntax();

    [GeneratedRegex(@"^[+-]?([0-9]+\.?[0-9]*|\.[0-9]+)([eE][+-]?[0-9]+)?$")]
    private static partial Regex FloatSyntax();

    public static object? ToInteger(object? value, IntegerType type)
    {
        if (SqlValue.IsNull(value))
            return null;

        switch (value)
        {
            case long integer:
                return Checked(integer, type);
            case double number:
                return Checked(TruncateToLong(number, type), type);
            case string text:
                return ParseInteger(text, type);
            default:
                throw MsSqlRaisedErrors.ConversionFailed(SqlValue.ToText(value!), type.Name);
        }
    }

    public static object? ToDecimal(object? value, int precision, int scale)
    {
        if (SqlValue.IsNull(value))
            return null;

        switch (value)
        {
            case long integer:
                return FitDecimal((decimal)integer, precision, scale);
            case double number:
                return FitDecimal(DecimalFromDouble(number), precision, scale);
            case string text:
                return FitDecimal(ParseDecimal(text), precision, scale);
            default:
                throw MsSqlRaisedErrors.ErrorConvertingDataType("numeric");
        }
    }

    public static object? ToFloat(object? value, string typeName)
    {
        if (SqlValue.IsNull(value))
            return null;

        switch (value)
        {
            case long integer:
                return (double)integer;
            case double number:
                return number;
            case string text:
                return ParseFloat(text, typeName);
            default:
                throw MsSqlRaisedErrors.ErrorConvertingDataType(typeName);
        }
    }

    public static object? ToBit(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        switch (value)
        {
            case long integer:
                return integer != 0 ? 1L : 0L;
            case double number:
                return number != 0 ? 1L : 0L;
            case string text:
                return ParseBit(text);
            default:
                throw MsSqlRaisedErrors.ConversionFailed(SqlValue.ToText(value!), "bit");
        }
    }

    public static bool TryParseNumber(string text, out double number)
    {
        string trimmed = text.Trim();
        if (FloatSyntax().IsMatch(trimmed))
        {
            number = double.Parse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture);
            return !double.IsInfinity(number);
        }

        number = 0;
        return false;
    }

    private static long Checked(long value, IntegerType type)
    {
        if (type.Contains(value))
            return value;
        throw MsSqlRaisedErrors.ArithmeticOverflow(type.Name);
    }

    private static long TruncateToLong(double number, IntegerType type)
    {
        double truncated = Math.Truncate(number);
        if (double.IsNaN(truncated) || Math.Abs(truncated) >= LongRangeLimit)
            throw MsSqlRaisedErrors.ArithmeticOverflow(type.Name);
        return (long)truncated;
    }

    private static long ParseInteger(string text, IntegerType type)
    {
        string trimmed = text.Trim();
        if (trimmed.Length == 0)
            return 0;
        if (!IntegerSyntax().IsMatch(trimmed))
            throw MsSqlRaisedErrors.ConversionFailed(text, type.Name);

        bool parsed = long.TryParse(
            trimmed,
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out long value
        );
        if (parsed && type.Contains(value))
            return value;
        if (type.IsWide)
            throw MsSqlRaisedErrors.ErrorConvertingDataType(type.Name);
        throw MsSqlRaisedErrors.ConversionOverflow(text, type.Name);
    }

    private static decimal ParseDecimal(string text)
    {
        string trimmed = text.Trim();
        if (!DecimalSyntax().IsMatch(trimmed))
            throw MsSqlRaisedErrors.ErrorConvertingDataType("numeric");

        bool parsed = decimal.TryParse(
            trimmed,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal value
        );
        if (!parsed)
            throw MsSqlRaisedErrors.NumericOverflow();
        return value;
    }

    private static double ParseFloat(string text, string typeName)
    {
        string trimmed = text.Trim();
        if (trimmed.Length == 0)
            return 0;
        if (!TryParseNumber(trimmed, out double number))
            throw MsSqlRaisedErrors.ErrorConvertingDataType(typeName);
        return number;
    }

    private static long ParseBit(string text)
    {
        string trimmed = text.Trim();
        if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
            return 0;
        if (trimmed.Length == 0)
            return 0;
        if (!TryParseNumber(trimmed, out double number))
            throw MsSqlRaisedErrors.ConversionFailed(text, "bit");
        return number != 0 ? 1 : 0;
    }

    private static decimal DecimalFromDouble(double number)
    {
        if (double.IsNaN(number) || Math.Abs(number) >= (double)DecimalRangeLimit)
            throw MsSqlRaisedErrors.NumericOverflow();
        return (decimal)number;
    }

    private static double FitDecimal(decimal value, int precision, int scale)
    {
        decimal rounded = Math.Round(
            value,
            Math.Min(scale, MaxDecimalScale),
            MidpointRounding.AwayFromZero
        );
        double limit = Math.Pow(10, precision - scale);
        if (Math.Abs((double)rounded) >= limit)
            throw MsSqlRaisedErrors.NumericOverflow();
        return (double)rounded;
    }
}

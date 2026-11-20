using System.Globalization;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Dialect.Functions;

public sealed class ConversionFunctions
{
    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastDate,
            1,
            arguments => CastDate(arguments[0], null)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastDate,
            2,
            arguments => CastDate(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastDateTime,
            2,
            arguments => CastDateTime(arguments[0], arguments[1], null)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastDateTime,
            3,
            arguments => CastDateTime(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastTime,
            2,
            arguments => CastTime(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastBit,
            1,
            arguments => CastBit(arguments[0])
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastTinyInt,
            1,
            arguments => NumericConversions.ToInteger(arguments[0], IntegerType.TinyInt)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastSmallInt,
            1,
            arguments => NumericConversions.ToInteger(arguments[0], IntegerType.SmallInt)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastInt,
            1,
            arguments => NumericConversions.ToInteger(arguments[0], IntegerType.Int)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastBigInt,
            1,
            arguments => NumericConversions.ToInteger(arguments[0], IntegerType.BigInt)
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastDecimal,
            3,
            arguments =>
                NumericConversions.ToDecimal(
                    arguments[0],
                    checked((int)SqlValue.ToLong(arguments[1]!)),
                    checked((int)SqlValue.ToLong(arguments[2]!))
                )
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastFloat,
            1,
            arguments => NumericConversions.ToFloat(arguments[0], "float")
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.CastReal,
            1,
            arguments => NumericConversions.ToFloat(arguments[0], "real")
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.FormatWithStyle,
            2,
            arguments => FormatWithStyle(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "FORMAT",
            2,
            arguments => Format(arguments[0], arguments[1], null)
        );
        yield return new SqliteScalarFunction(
            "FORMAT",
            3,
            arguments => Format(arguments[0], arguments[1], arguments[2])
        );
    }

    public static object? CastDate(object? value, object? style)
    {
        if (SqlValue.IsNull(value))
            return null;

        return SqlDateFormatter.FormatDate(ParseDate(value!, style).Value.Date);
    }

    public static object? CastDateTime(object? value, object? fractionDigits, object? style)
    {
        if (SqlValue.IsNull(value))
            return null;

        int digits = checked((int)SqlValue.ToLong(fractionDigits!));
        return SqlDateFormatter.FormatDateTime(ParseDate(value!, style).Value, digits);
    }

    public static object? CastTime(object? value, object? fractionDigits)
    {
        if (SqlValue.IsNull(value))
            return null;

        int digits = checked((int)SqlValue.ToLong(fractionDigits!));
        return SqlDateFormatter.FormatTime(ParseDate(value!, null).Value, digits);
    }

    public static object? CastBit(object? value)
    {
        return NumericConversions.ToBit(value);
    }

    public static object? FormatWithStyle(object? value, object? style)
    {
        if (SqlValue.IsNull(value))
            return null;

        int code = checked((int)SqlValue.ToLong(style!));
        return ConvertStyleFormats.Format(ParseDate(value!, null).Value, code);
    }

    public static object? Format(object? value, object? pattern, object? culture)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(pattern))
            return null;

        CultureInfo cultureInfo = ResolveCulture(culture);
        string format = SqlValue.ToText(pattern!);

        switch (value)
        {
            case long integer:
                return integer.ToString(format, cultureInfo);
            case double number:
                return number.ToString(format, cultureInfo);
            case string text when SqlDateParser.TryParse(text, null, out SqlDateValue date):
                return date.Value.ToString(format, cultureInfo);
            default:
                return SqlValue.ToText(value!);
        }
    }

    private static SqlDateValue ParseDate(object value, object? style)
    {
        int? styleCode = SqlValue.IsNull(style) ? null : checked((int)SqlValue.ToLong(style!));
        if (SqlDateParser.TryParse(value, styleCode, out SqlDateValue parsed))
            return parsed;

        throw MsSqlRaisedErrors.DateConversionFailed();
    }

    private static CultureInfo ResolveCulture(object? culture)
    {
        if (SqlValue.IsNull(culture))
            return FormatCultures.Default;

        string name = SqlValue.ToText(culture!);
        if (FormatCultures.TryGet(name, out CultureInfo? resolved))
            return resolved;

        throw MsSqlRaisedErrors.UnsupportedCulture(name);
    }
}

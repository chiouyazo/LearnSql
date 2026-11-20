using System.Globalization;
using LearnSql.Sql.Dialect.Functions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting;

public static class SqliteCastBuilder
{
    public const int ConversionTextLength = 30;
    public const int VariableTextLength = 1;

    private const int DefaultTimeScale = 7;
    private const int DefaultPrecision = 18;

    public static string Build(
        string valueSql,
        DataTypeReference? target,
        string? style = null,
        int defaultTextLength = ConversionTextLength
    )
    {
        if (target is not SqlDataTypeReference sqlType)
            return valueSql;

        switch (sqlType.SqlDataTypeOption)
        {
            case SqlDataTypeOption.TinyInt:
                return Call(InternalFunctionNames.CastTinyInt, valueSql);
            case SqlDataTypeOption.SmallInt:
                return Call(InternalFunctionNames.CastSmallInt, valueSql);
            case SqlDataTypeOption.Int:
                return Call(InternalFunctionNames.CastInt, valueSql);
            case SqlDataTypeOption.BigInt:
                return Call(InternalFunctionNames.CastBigInt, valueSql);
            case SqlDataTypeOption.Bit:
                return Call(InternalFunctionNames.CastBit, valueSql);
            case SqlDataTypeOption.Decimal:
            case SqlDataTypeOption.Numeric:
                return Call(
                    InternalFunctionNames.CastDecimal,
                    valueSql,
                    ParameterAt(sqlType, 0, DefaultPrecision)
                        .ToString(CultureInfo.InvariantCulture),
                    ParameterAt(sqlType, 1, 0).ToString(CultureInfo.InvariantCulture)
                );
            case SqlDataTypeOption.Money:
            case SqlDataTypeOption.SmallMoney:
                return "ROUND(CAST(" + valueSql + " AS NUMERIC), 4)";
            case SqlDataTypeOption.Float:
                return Call(InternalFunctionNames.CastFloat, valueSql);
            case SqlDataTypeOption.Real:
                return Call(InternalFunctionNames.CastReal, valueSql);
            case SqlDataTypeOption.Date:
                return InternalFunctionNames.CastDate
                    + "("
                    + valueSql
                    + ArgumentSuffix(style)
                    + ")";
            case SqlDataTypeOption.DateTime:
                return DateTimeCast(valueSql, 3, style);
            case SqlDataTypeOption.SmallDateTime:
                return DateTimeCast(valueSql, 0, style);
            case SqlDataTypeOption.DateTime2:
                return DateTimeCast(valueSql, ParameterAt(sqlType, 0, DefaultTimeScale), style);
            case SqlDataTypeOption.Time:
                return InternalFunctionNames.CastTime
                    + "("
                    + valueSql
                    + ", "
                    + ParameterAt(sqlType, 0, DefaultTimeScale)
                    + ")";
            case SqlDataTypeOption.Char:
            case SqlDataTypeOption.VarChar:
            case SqlDataTypeOption.NChar:
            case SqlDataTypeOption.NVarChar:
                return TextCast(valueSql, sqlType, style, defaultTextLength);
            case SqlDataTypeOption.Text:
            case SqlDataTypeOption.NText:
            case SqlDataTypeOption.UniqueIdentifier:
                return "CAST(" + valueSql + " AS TEXT)";
            case SqlDataTypeOption.Binary:
            case SqlDataTypeOption.VarBinary:
            case SqlDataTypeOption.Image:
                return "CAST(" + valueSql + " AS BLOB)";
            default:
                return valueSql;
        }
    }

    private static string DateTimeCast(string valueSql, int fractionDigits, string? style)
    {
        return InternalFunctionNames.CastDateTime
            + "("
            + valueSql
            + ", "
            + fractionDigits
            + ArgumentSuffix(style)
            + ")";
    }

    private static string Call(string function, params string[] arguments)
    {
        return function + "(" + string.Join(", ", arguments) + ")";
    }

    private static string TextCast(
        string valueSql,
        SqlDataTypeReference sqlType,
        string? style,
        int defaultTextLength
    )
    {
        string converted = style is null
            ? "CAST(" + valueSql + " AS TEXT)"
            : InternalFunctionNames.FormatWithStyle + "(" + valueSql + ", " + style + ")";

        int? length = TextLengthOf(sqlType, defaultTextLength);
        if (length is null)
            return converted;

        bool fixedLength =
            sqlType.SqlDataTypeOption is SqlDataTypeOption.Char or SqlDataTypeOption.NChar;
        string padded = fixedLength
            ? "(" + converted + " || PRINTF('%" + length.Value + "s', ''))"
            : converted;
        return "SUBSTR(" + padded + ", 1, " + length.Value + ")";
    }

    private static string ArgumentSuffix(string? style)
    {
        return style is null ? string.Empty : ", " + style;
    }

    private static int? TextLengthOf(SqlDataTypeReference sqlType, int defaultLength)
    {
        if (sqlType.Parameters.Count == 0)
            return defaultLength;

        Literal first = sqlType.Parameters[0];
        return first is IntegerLiteral && int.TryParse(first.Value, out int length) ? length : null;
    }

    private static int ParameterAt(SqlDataTypeReference sqlType, int index, int fallback)
    {
        if (
            index < sqlType.Parameters.Count
            && int.TryParse(sqlType.Parameters[index].Value, out int value)
        )
            return value;
        return fallback;
    }
}

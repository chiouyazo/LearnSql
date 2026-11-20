using System.Globalization;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Dialect.Functions;

public sealed class ArithmeticFunctions
{
    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction(
            InternalFunctionNames.Divisor,
            1,
            arguments => Divisor(arguments[0])
        );
        yield return new SqliteScalarFunction(
            InternalFunctionNames.Modulo,
            2,
            arguments => Modulo(arguments[0], arguments[1])
        );
    }

    public static object? Divisor(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        bool isZero = value switch
        {
            long integer => integer == 0,
            double number => number == 0,
            string text => NumericConversions.TryParseNumber(text, out double parsed)
                && parsed == 0,
            _ => false,
        };
        if (isZero)
            throw MsSqlRaisedErrors.DivideByZero();

        return value;
    }

    public static object? Modulo(object? dividend, object? divisor)
    {
        if (SqlValue.IsNull(dividend) || SqlValue.IsNull(divisor))
            return null;

        object left = Coerce(dividend!);
        object right = Coerce(divisor!);
        if (left is long leftInteger && right is long rightInteger)
        {
            if (rightInteger == 0)
                throw MsSqlRaisedErrors.DivideByZero();
            return rightInteger == -1 ? 0L : leftInteger % rightInteger;
        }

        double leftNumber = Convert.ToDouble(left);
        double rightNumber = Convert.ToDouble(right);
        if (rightNumber == 0)
            throw MsSqlRaisedErrors.DivideByZero();
        return leftNumber % rightNumber;
    }

    private static object Coerce(object value)
    {
        switch (value)
        {
            case long or double:
                return value;
            case string text:
                return CoerceText(text);
            default:
                throw SqlValue.ConversionFailed(SqlValue.ToText(value), "int");
        }
    }

    private static object CoerceText(string text)
    {
        string trimmed = text.Trim();
        if (
            long.TryParse(
                trimmed,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out long integer
            )
        )
            return integer;
        if (NumericConversions.TryParseNumber(trimmed, out double number))
            return number;
        throw SqlValue.ConversionFailed(text, "int");
    }
}

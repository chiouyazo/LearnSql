namespace LearnSql.Sql.Dialect.Functions;

public sealed class MathFunctions
{
    private const string InvalidOperationMessage = "An invalid floating point operation occurred.";

    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction(
            "CEILING",
            1,
            arguments => Round(arguments[0], Math.Ceiling)
        );
        yield return new SqliteScalarFunction(
            "FLOOR",
            1,
            arguments => Round(arguments[0], Math.Floor)
        );
        yield return new SqliteScalarFunction(
            "POWER",
            2,
            arguments => Power(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction("SQRT", 1, arguments => Sqrt(arguments[0]));
        yield return new SqliteScalarFunction("SQUARE", 1, arguments => Square(arguments[0]));
        yield return new SqliteScalarFunction("SIGN", 1, arguments => Sign(arguments[0]));
        yield return new SqliteScalarFunction("LOG", 1, arguments => Log(arguments[0]));
        yield return new SqliteScalarFunction(
            "LOG",
            2,
            arguments => Log(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction("LOG10", 1, arguments => Log10(arguments[0]));
        yield return new SqliteScalarFunction("EXP", 1, arguments => Map(arguments[0], Math.Exp));
        yield return new SqliteScalarFunction("PI", 0, _ => Math.PI);
        yield return new SqliteScalarFunction(
            "ROUND",
            2,
            arguments => RoundFunction.Round(arguments[0], arguments[1], null)
        );
        yield return new SqliteScalarFunction(
            "ROUND",
            3,
            arguments => RoundFunction.Round(arguments[0], arguments[1], arguments[2])
        );
    }

    public static object? Round(object? value, Func<double, double> rounding)
    {
        if (SqlValue.IsNull(value))
            return null;
        if (value is long integer)
            return integer;
        return rounding(SqlValue.ToDouble(value!));
    }

    public static object? Power(object? baseValue, object? exponent)
    {
        if (SqlValue.IsNull(baseValue) || SqlValue.IsNull(exponent))
            return null;

        if (
            baseValue is long integerBase
            && exponent is long integerExponent
            && integerExponent >= 0
        )
        {
            long result = 1;
            for (long step = 0; step < integerExponent; step++)
                result = checked(result * integerBase);
            return result;
        }

        return Math.Pow(SqlValue.ToDouble(baseValue!), SqlValue.ToDouble(exponent!));
    }

    public static object? Sqrt(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        double number = SqlValue.ToDouble(value!);
        if (number < 0)
            throw new ArgumentException(InvalidOperationMessage);
        return Math.Sqrt(number);
    }

    public static object? Square(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;
        if (value is long integer)
            return checked(integer * integer);

        double number = SqlValue.ToDouble(value!);
        return number * number;
    }

    public static object? Sign(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;
        if (value is long integer)
            return (long)Math.Sign(integer);
        return (double)Math.Sign(SqlValue.ToDouble(value!));
    }

    public static object? Log(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        return Math.Log(PositiveNumber(value!));
    }

    public static object? Log(object? value, object? logBase)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(logBase))
            return null;

        double baseNumber = SqlValue.ToDouble(logBase!);
        if (baseNumber <= 0 || baseNumber == 1)
            throw new ArgumentException(InvalidOperationMessage);
        return Math.Log(PositiveNumber(value!), baseNumber);
    }

    public static object? Log10(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        return Math.Log10(PositiveNumber(value!));
    }

    private static double PositiveNumber(object value)
    {
        double number = SqlValue.ToDouble(value);
        if (number <= 0)
            throw new ArgumentException(InvalidOperationMessage);
        return number;
    }

    private static object? Map(object? value, Func<double, double> function)
    {
        return SqlValue.IsNull(value) ? null : function(SqlValue.ToDouble(value!));
    }
}

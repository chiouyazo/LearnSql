namespace LearnSql.Sql.Dialect.Functions;

public static class RoundFunction
{
    private const int MaxDecimalDigits = 28;

    public static object? Round(object? value, object? digits, object? mode)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(digits))
            return null;

        int precision = checked((int)SqlValue.ToLong(digits!));
        bool truncate = !SqlValue.IsNull(mode) && SqlValue.ToLong(mode!) != 0;

        if (value is long integer)
            return RoundInteger(integer, precision, truncate);

        return RoundDouble(SqlValue.ToDouble(value!), precision, truncate);
    }

    private static long RoundInteger(long value, int precision, bool truncate)
    {
        if (precision >= 0)
            return value;
        if (-precision > 18)
            return 0;

        long factor = (long)Math.Pow(10, -precision);
        decimal scaled = (decimal)value / factor;
        decimal rounded = truncate
            ? Math.Truncate(scaled)
            : Math.Round(scaled, 0, MidpointRounding.AwayFromZero);
        return checked((long)rounded * factor);
    }

    private static double RoundDouble(double value, int precision, bool truncate)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return value;

        try
        {
            decimal exact = (decimal)value;
            if (precision >= 0)
            {
                int digits = Math.Min(precision, MaxDecimalDigits);
                decimal result = truncate
                    ? TruncateDecimal(exact, digits)
                    : Math.Round(exact, digits, MidpointRounding.AwayFromZero);
                return (double)result;
            }

            decimal factor = (decimal)Math.Pow(10, -precision);
            decimal scaled = exact / factor;
            decimal rounded = truncate
                ? Math.Truncate(scaled)
                : Math.Round(scaled, 0, MidpointRounding.AwayFromZero);
            return (double)(rounded * factor);
        }
        catch (OverflowException)
        {
            return value;
        }
    }

    private static decimal TruncateDecimal(decimal value, int digits)
    {
        decimal factor = (decimal)Math.Pow(10, digits);
        return Math.Truncate(value * factor) / factor;
    }
}

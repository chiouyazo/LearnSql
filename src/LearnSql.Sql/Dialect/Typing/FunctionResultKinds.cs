namespace LearnSql.Sql.Dialect.Typing;

public static class FunctionResultKinds
{
    private static readonly HashSet<string> TextFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "UPPER",
        "LOWER",
        "LTRIM",
        "RTRIM",
        "TRIM",
        "SUBSTRING",
        "REPLACE",
        "REPLICATE",
        "SPACE",
        "STUFF",
        "REVERSE",
        "CONCAT",
        "CONCAT_WS",
        "FORMAT",
        "DATENAME",
        "CHAR",
        "NCHAR",
        "STR",
        "STRING_AGG",
        "NEWID",
        "QUOTENAME",
        "TRANSLATE",
    };

    private static readonly HashSet<string> NumericFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "COUNT",
        "COUNT_BIG",
        "LEN",
        "CHARINDEX",
        "PATINDEX",
        "DATEDIFF",
        "DATEDIFF_BIG",
        "DATEPART",
        "YEAR",
        "MONTH",
        "DAY",
        "ASCII",
        "UNICODE",
        "SUM",
        "AVG",
        "POWER",
        "SQRT",
        "SQUARE",
        "SIGN",
        "LOG",
        "LOG10",
        "EXP",
        "PI",
        "ROW_NUMBER",
        "RANK",
        "DENSE_RANK",
        "NTILE",
        "SCOPE_IDENTITY",
    };

    private static readonly HashSet<string> DateTimeFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "GETDATE",
        "GETUTCDATE",
        "SYSDATETIME",
        "SYSUTCDATETIME",
        "EOMONTH",
        "DATEFROMPARTS",
    };

    private static readonly HashSet<string> FirstArgumentTypedFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "MIN",
        "MAX",
        "ABS",
        "ROUND",
        "CEILING",
        "FLOOR",
        "NULLIF",
        "LAG",
        "LEAD",
        "FIRST_VALUE",
        "LAST_VALUE",
    };

    private static readonly HashSet<string> ThirdArgumentTypedFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "DATEADD",
    };

    private static readonly HashSet<string> NullReplacementFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "ISNULL",
    };

    public static bool TryGetFixedKind(string functionName, out ValueKind kind)
    {
        if (TextFunctions.Contains(functionName))
            kind = ValueKind.Text;
        else if (NumericFunctions.Contains(functionName))
            kind = ValueKind.Numeric;
        else if (DateTimeFunctions.Contains(functionName))
            kind = ValueKind.DateTime;
        else
        {
            kind = ValueKind.Unknown;
            return false;
        }
        return true;
    }

    public static bool IsFirstArgumentTyped(string functionName)
    {
        return FirstArgumentTypedFunctions.Contains(functionName);
    }

    public static bool IsThirdArgumentTyped(string functionName)
    {
        return ThirdArgumentTypedFunctions.Contains(functionName);
    }

    public static bool IsNullReplacement(string functionName)
    {
        return NullReplacementFunctions.Contains(functionName);
    }
}

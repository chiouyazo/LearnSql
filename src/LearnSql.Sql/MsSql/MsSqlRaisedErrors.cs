namespace LearnSql.Sql.MsSql;

public static class MsSqlRaisedErrors
{
    public const string SourceTypeArgument = "SourceType";
    public const string TargetTypeArgument = "TargetType";
    public const string ValueArgument = "Value";
    public const string ColumnTypeArgument = "ColumnType";
    public const string CultureArgument = "Culture";

    public static MsSqlRaisedException ConversionFailed(string value, string targetType)
    {
        return new MsSqlRaisedException(
            MsSqlRaisedErrorKind.ConversionFailed,
            new Dictionary<string, string>
            {
                [SourceTypeArgument] = "varchar",
                [ValueArgument] = value,
                [TargetTypeArgument] = targetType,
            }
        );
    }

    public static MsSqlRaisedException ConversionOverflow(string value, string targetType)
    {
        string article = "aeiou".Contains(targetType[0]) ? "an" : "a";
        return new MsSqlRaisedException(
            MsSqlRaisedErrorKind.ConversionOverflow,
            new Dictionary<string, string>
            {
                [SourceTypeArgument] = "varchar",
                [ValueArgument] = value,
                [ColumnTypeArgument] = article + " " + targetType,
            }
        );
    }

    public static MsSqlRaisedException DateConversionFailed()
    {
        return new MsSqlRaisedException(MsSqlRaisedErrorKind.DateConversionFailed);
    }

    public static MsSqlRaisedException ErrorConvertingDataType(string targetType)
    {
        return new MsSqlRaisedException(
            MsSqlRaisedErrorKind.ErrorConvertingDataType,
            new Dictionary<string, string>
            {
                [SourceTypeArgument] = "varchar",
                [TargetTypeArgument] = targetType,
            }
        );
    }

    public static MsSqlRaisedException ArithmeticOverflow(string targetType)
    {
        return new MsSqlRaisedException(
            MsSqlRaisedErrorKind.ArithmeticOverflow,
            new Dictionary<string, string> { [TargetTypeArgument] = targetType }
        );
    }

    public static MsSqlRaisedException NumericOverflow()
    {
        return new MsSqlRaisedException(MsSqlRaisedErrorKind.NumericOverflow);
    }

    public static MsSqlRaisedException DivideByZero()
    {
        return new MsSqlRaisedException(MsSqlRaisedErrorKind.DivideByZero);
    }

    public static MsSqlRaisedException UnsupportedCulture(string culture)
    {
        return new MsSqlRaisedException(
            MsSqlRaisedErrorKind.UnsupportedCulture,
            new Dictionary<string, string> { [CultureArgument] = culture }
        );
    }
}

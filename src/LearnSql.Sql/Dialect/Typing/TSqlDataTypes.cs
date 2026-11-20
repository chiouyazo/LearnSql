using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Typing;

public static class TSqlDataTypes
{
    private static readonly HashSet<SqlDataTypeOption> SupportedOptions =
    [
        SqlDataTypeOption.BigInt,
        SqlDataTypeOption.Int,
        SqlDataTypeOption.SmallInt,
        SqlDataTypeOption.TinyInt,
        SqlDataTypeOption.Bit,
        SqlDataTypeOption.Decimal,
        SqlDataTypeOption.Numeric,
        SqlDataTypeOption.Money,
        SqlDataTypeOption.SmallMoney,
        SqlDataTypeOption.Float,
        SqlDataTypeOption.Real,
        SqlDataTypeOption.DateTime,
        SqlDataTypeOption.SmallDateTime,
        SqlDataTypeOption.DateTime2,
        SqlDataTypeOption.Date,
        SqlDataTypeOption.Time,
        SqlDataTypeOption.Char,
        SqlDataTypeOption.VarChar,
        SqlDataTypeOption.Text,
        SqlDataTypeOption.NChar,
        SqlDataTypeOption.NVarChar,
        SqlDataTypeOption.NText,
        SqlDataTypeOption.Binary,
        SqlDataTypeOption.VarBinary,
        SqlDataTypeOption.Image,
        SqlDataTypeOption.UniqueIdentifier,
    ];

    public static bool IsSupported(SqlDataTypeOption option)
    {
        return SupportedOptions.Contains(option);
    }

    public static ValueKind GetKind(DataTypeReference? dataType)
    {
        if (dataType is not SqlDataTypeReference sqlType)
            return ValueKind.Unknown;

        switch (sqlType.SqlDataTypeOption)
        {
            case SqlDataTypeOption.BigInt:
            case SqlDataTypeOption.Int:
            case SqlDataTypeOption.SmallInt:
            case SqlDataTypeOption.TinyInt:
            case SqlDataTypeOption.Bit:
            case SqlDataTypeOption.Decimal:
            case SqlDataTypeOption.Numeric:
            case SqlDataTypeOption.Money:
            case SqlDataTypeOption.SmallMoney:
            case SqlDataTypeOption.Float:
            case SqlDataTypeOption.Real:
                return ValueKind.Numeric;
            case SqlDataTypeOption.DateTime:
            case SqlDataTypeOption.SmallDateTime:
            case SqlDataTypeOption.DateTime2:
            case SqlDataTypeOption.Date:
            case SqlDataTypeOption.Time:
                return ValueKind.DateTime;
            case SqlDataTypeOption.Char:
            case SqlDataTypeOption.VarChar:
            case SqlDataTypeOption.Text:
            case SqlDataTypeOption.NChar:
            case SqlDataTypeOption.NVarChar:
            case SqlDataTypeOption.NText:
            case SqlDataTypeOption.UniqueIdentifier:
                return ValueKind.Text;
            case SqlDataTypeOption.Binary:
            case SqlDataTypeOption.VarBinary:
            case SqlDataTypeOption.Image:
                return ValueKind.Binary;
            default:
                return ValueKind.Unknown;
        }
    }
}

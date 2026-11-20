using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public static class IdentityColumns
{
    public static bool IsTranslatable(ColumnDefinition column)
    {
        if (column.IdentityOptions is null)
            return true;

        return IsIntegerType(column.DataType) && FindPrimaryKey(column) is not null;
    }

    public static UniqueConstraintDefinition? FindPrimaryKey(ColumnDefinition column)
    {
        return column
            .Constraints.OfType<UniqueConstraintDefinition>()
            .FirstOrDefault(constraint => constraint.IsPrimaryKey);
    }

    private static bool IsIntegerType(DataTypeReference dataType)
    {
        return dataType
            is SqlDataTypeReference
            {
                SqlDataTypeOption: SqlDataTypeOption.TinyInt
                    or SqlDataTypeOption.SmallInt
                    or SqlDataTypeOption.Int
                    or SqlDataTypeOption.BigInt,
            };
    }
}

namespace LearnSql.Sql.Dialect.Functions;

public sealed record IntegerType(string Name, long Minimum, long Maximum)
{
    public static readonly IntegerType TinyInt = new IntegerType("tinyint", 0, byte.MaxValue);
    public static readonly IntegerType SmallInt = new IntegerType(
        "smallint",
        short.MinValue,
        short.MaxValue
    );
    public static readonly IntegerType Int = new IntegerType("int", int.MinValue, int.MaxValue);
    public static readonly IntegerType BigInt = new IntegerType(
        "bigint",
        long.MinValue,
        long.MaxValue
    );

    public bool IsWide => Maximum == long.MaxValue;

    public bool Contains(long value)
    {
        return value >= Minimum && value <= Maximum;
    }
}

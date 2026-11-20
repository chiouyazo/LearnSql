namespace LearnSql.Sql.Dialect.Functions;

public sealed class GeneralFunctions
{
    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction(
            "ISNULL",
            2,
            arguments => IsNull(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "NEWID",
            0,
            _ => Guid.NewGuid().ToString().ToUpperInvariant(),
            false
        );
    }

    public static object? IsNull(object? value, object? replacement)
    {
        return SqlValue.IsNull(value) ? replacement : value;
    }
}

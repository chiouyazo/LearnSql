using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Dialect.Functions;

public sealed class SqliteFunctionRegistrar
{
    private readonly IReadOnlyList<SqliteScalarFunction> _functions;

    public SqliteFunctionRegistrar(TimeProvider? timeProvider = null)
    {
        TimeProvider provider = timeProvider ?? TimeProvider.System;
        _functions =
        [
            .. new DateFunctions(provider).Create(),
            .. new StringFunctions().Create(),
            .. new MathFunctions().Create(),
            .. new ArithmeticFunctions().Create(),
            .. new ConversionFunctions().Create(),
            .. new GeneralFunctions().Create(),
        ];
    }

    public IReadOnlyList<SqliteScalarFunction> Functions => _functions;

    public void Register(SqliteConnection connection)
    {
        foreach (SqliteScalarFunction function in _functions)
            function.RegisterOn(connection);
    }
}

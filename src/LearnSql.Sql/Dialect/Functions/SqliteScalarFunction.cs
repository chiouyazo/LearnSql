using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Dialect.Functions;

public sealed record SqliteScalarFunction(
    string Name,
    int ArgumentCount,
    Func<object?[], object?> Implementation,
    bool IsDeterministic = true
)
{
    public void RegisterOn(SqliteConnection connection)
    {
        Func<object?[], object?> implementation = Implementation;
        switch (ArgumentCount)
        {
            case 0:
                connection.CreateFunction<object?>(Name, () => implementation([]), IsDeterministic);
                break;
            case 1:
                connection.CreateFunction<object?, object?>(
                    Name,
                    a => implementation([a]),
                    IsDeterministic
                );
                break;
            case 2:
                connection.CreateFunction<object?, object?, object?>(
                    Name,
                    (a, b) => implementation([a, b]),
                    IsDeterministic
                );
                break;
            case 3:
                connection.CreateFunction<object?, object?, object?, object?>(
                    Name,
                    (a, b, c) => implementation([a, b, c]),
                    IsDeterministic
                );
                break;
            case 4:
                connection.CreateFunction<object?, object?, object?, object?, object?>(
                    Name,
                    (a, b, c, d) => implementation([a, b, c, d]),
                    IsDeterministic
                );
                break;
            default:
                throw new NotSupportedException(
                    $"Functions with {ArgumentCount} arguments are not supported."
                );
        }
    }
}

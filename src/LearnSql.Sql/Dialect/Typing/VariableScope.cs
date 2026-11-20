using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Typing;

public sealed class VariableScope
{
    private readonly Dictionary<string, DeclaredVariable> _variables = new Dictionary<
        string,
        DeclaredVariable
    >(StringComparer.OrdinalIgnoreCase);

    public bool IsEmpty => _variables.Count == 0;

    public void Declare(
        string name,
        DataTypeReference dataType,
        DeclareVariableStatement declaredBy
    )
    {
        _variables[name] = new DeclaredVariable(
            name,
            dataType,
            TSqlDataTypes.GetKind(dataType),
            declaredBy
        );
    }

    public bool TryGet(string name, out DeclaredVariable? variable)
    {
        bool found = _variables.TryGetValue(name, out DeclaredVariable? value);
        variable = value;
        return found;
    }
}

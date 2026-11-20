using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Typing;

public sealed record DeclaredVariable(
    string Name,
    DataTypeReference DataType,
    ValueKind Kind,
    DeclareVariableStatement DeclaredBy
);

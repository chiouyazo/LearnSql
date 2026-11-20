using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public sealed class StatementProfiler
{
    public StatementProfile Profile(TSqlStatement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);

        return statement switch
        {
            SelectStatement { Into: not null } select => Single(
                StatementKind.SelectInto,
                select.Into.BaseIdentifier.Value
            ),
            SelectStatement => new StatementProfile(StatementKind.Select, null, []),
            InsertStatement insert => ForTarget(
                StatementKind.Insert,
                insert.InsertSpecification.Target
            ),
            UpdateStatement update => ForTarget(
                StatementKind.Update,
                update.UpdateSpecification.Target
            ),
            DeleteStatement delete => ForTarget(
                StatementKind.Delete,
                delete.DeleteSpecification.Target
            ),
            TruncateTableStatement truncate => Single(
                StatementKind.TruncateTable,
                truncate.TableName.BaseIdentifier.Value
            ),
            CreateTableStatement create => Single(
                StatementKind.CreateTable,
                create.SchemaObjectName.BaseIdentifier.Value
            ),
            AlterTableStatement alter => Single(
                StatementKind.AlterTable,
                alter.SchemaObjectName.BaseIdentifier.Value
            ),
            DropTableStatement drop => Multiple(StatementKind.DropTable, drop.Objects),
            CreateViewStatement view => Single(
                StatementKind.CreateView,
                view.SchemaObjectName.BaseIdentifier.Value
            ),
            DropViewStatement dropView => Multiple(StatementKind.DropView, dropView.Objects),
            CreateIndexStatement index => ForIndex(index),
            DropIndexStatement dropIndex => ForDropIndex(dropIndex),
            BeginTransactionStatement => Plain(StatementKind.BeginTransaction),
            CommitTransactionStatement => Plain(StatementKind.CommitTransaction),
            RollbackTransactionStatement => Plain(StatementKind.RollbackTransaction),
            SaveTransactionStatement => Plain(StatementKind.SaveTransaction),
            DeclareVariableStatement => Plain(StatementKind.DeclareVariable),
            SetVariableStatement => Plain(StatementKind.SetVariable),
            PredicateSetStatement => Plain(StatementKind.SetOption),
            _ => StatementProfile.Unknown,
        };
    }

    private static StatementProfile Plain(StatementKind kind)
    {
        return new StatementProfile(kind, null, []);
    }

    private static StatementProfile Single(StatementKind kind, string table)
    {
        return new StatementProfile(kind, table, [table]);
    }

    private static StatementProfile ForTarget(StatementKind kind, TableReference? target)
    {
        if (target is NamedTableReference named)
            return Single(kind, named.SchemaObject.BaseIdentifier.Value);

        return Plain(kind);
    }

    private static StatementProfile Multiple(StatementKind kind, IList<SchemaObjectName> objects)
    {
        List<string> names = [];
        foreach (SchemaObjectName name in objects)
            names.Add(name.BaseIdentifier.Value);

        return new StatementProfile(kind, names.Count > 0 ? names[0] : null, names);
    }

    private static StatementProfile ForIndex(CreateIndexStatement statement)
    {
        string table = statement.OnName.BaseIdentifier.Value;
        List<string> names = [table];
        if (statement.Name is not null)
            names.Add(statement.Name.Value);

        return new StatementProfile(StatementKind.CreateIndex, table, names);
    }

    private static StatementProfile ForDropIndex(DropIndexStatement statement)
    {
        string? table = null;
        List<string> names = [];
        foreach (DropIndexClauseBase clause in statement.DropIndexClauses)
        {
            switch (clause)
            {
                case DropIndexClause dropClause:
                    table ??= dropClause.Object?.BaseIdentifier.Value;
                    if (dropClause.Object is not null)
                        names.Add(dropClause.Object.BaseIdentifier.Value);
                    names.Add(dropClause.Index.Value);
                    break;
                case BackwardsCompatibleDropIndexClause legacy:
                    names.Add(legacy.Index.BaseIdentifier.Value);
                    break;
            }
        }

        return new StatementProfile(StatementKind.DropIndex, table, names);
    }
}

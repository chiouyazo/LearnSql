using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Translation;

namespace LearnSql.Sql.Execution;

public sealed class ForeignKeyConflictResolver
{
    public ForeignKeyConflict? Resolve(
        DatabaseSchema schema,
        StatementKind kind,
        string? targetTable
    )
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (targetTable is null || !kind.IsDataModification())
        {
            return null;
        }

        List<ForeignKeyConflict> candidates = [];
        if (kind is StatementKind.Insert or StatementKind.Update)
        {
            candidates.AddRange(ChildSide(schema, targetTable));
        }

        if (kind is StatementKind.Delete or StatementKind.Update)
        {
            candidates.AddRange(ParentSide(schema, targetTable));
        }

        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static IEnumerable<ForeignKeyConflict> ChildSide(DatabaseSchema schema, string table)
    {
        RelationSchema? relation = schema.Find(table);
        if (relation is null)
        {
            yield break;
        }

        foreach (ForeignKeySchema key in relation.ForeignKeys)
        {
            yield return Create(relation.Name, key);
        }
    }

    private static IEnumerable<ForeignKeyConflict> ParentSide(DatabaseSchema schema, string table)
    {
        foreach (RelationSchema relation in schema.Relations)
        {
            foreach (ForeignKeySchema key in relation.ForeignKeys)
            {
                if (string.Equals(key.ReferencedTable, table, StringComparison.OrdinalIgnoreCase))
                {
                    yield return Create(relation.Name, key);
                }
            }
        }
    }

    private static ForeignKeyConflict Create(string childTable, ForeignKeySchema key)
    {
        return new ForeignKeyConflict(
            key.Name,
            childTable,
            key.Columns.Count > 0 ? key.Columns[0] : string.Empty,
            key.ReferencedTable,
            key.ReferencedColumns.Count > 0 ? key.ReferencedColumns[0] : string.Empty
        );
    }
}

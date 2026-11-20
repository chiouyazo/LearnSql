using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Dialect.Typing;

public sealed class SqliteSchemaCatalogReader
{
    private const string Query =
        "SELECT m.name, p.name, p.type FROM sqlite_master m JOIN pragma_table_info(m.name) p "
        + "WHERE m.type IN ('table', 'view') ORDER BY m.name, p.cid";

    public SchemaCatalog Read(SqliteConnection connection)
    {
        Dictionary<string, List<ColumnInfo>> columnsByRelation = new Dictionary<
            string,
            List<ColumnInfo>
        >(StringComparer.OrdinalIgnoreCase);

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = Query;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string relation = reader.GetString(0);
                    string column = reader.GetString(1);
                    string declaredType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                    if (!columnsByRelation.TryGetValue(relation, out List<ColumnInfo>? columns))
                    {
                        columns = [];
                        columnsByRelation[relation] = columns;
                    }
                    columns.Add(
                        new ColumnInfo(column, DeclaredTypeClassifier.Classify(declaredType))
                    );
                }
            }
        }

        List<KeyValuePair<string, RelationShape>> relations = [];
        foreach (KeyValuePair<string, List<ColumnInfo>> entry in columnsByRelation)
            relations.Add(
                new KeyValuePair<string, RelationShape>(entry.Key, new RelationShape(entry.Value))
            );

        return new SchemaCatalog(relations);
    }
}

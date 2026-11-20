using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Execution;

public sealed class SqliteSchemaReader
{
    private const string RelationQuery =
        "SELECT name, type, sql, 0 FROM sqlite_master WHERE type IN ('table', 'view') "
        + "UNION ALL SELECT name, type, sql, 1 FROM sqlite_temp_master WHERE type IN ('table', 'view') "
        + "ORDER BY 4, 1";

    private const string ColumnQuery =
        "SELECT name, type, \"notnull\", dflt_value, pk FROM pragma_table_xinfo($name) "
        + "WHERE hidden IN (0, 2, 3) ORDER BY cid";

    private const string ForeignKeyQuery =
        "SELECT id, \"table\", \"from\", \"to\", on_update, on_delete FROM pragma_foreign_key_list($name) "
        + "ORDER BY id, seq";

    private const string InternalPrefix = "sqlite_";
    private const string IntegerType = "INTEGER";

    private static readonly string[] SystemViews = ["COLUMNS", "TABLES"];

    public DatabaseSchema Read(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        List<RelationHeader> headers = ReadHeaders(connection);
        List<RelationSchema> relations = [];
        foreach (RelationHeader header in headers)
        {
            List<ColumnSchema> columns = ReadColumns(connection, header);
            List<RawForeignKey> keys =
                header.Kind == RelationKind.Table ? ReadForeignKeys(connection, header) : [];
            relations.Add(
                new RelationSchema(
                    header.Name,
                    header.Kind,
                    IsSystem(header.Name),
                    header.IsTemporary,
                    columns,
                    ResolveForeignKeys(keys, header)
                )
            );
        }

        return new DatabaseSchema(ResolveReferencedColumns(relations));
    }

    private static List<RelationHeader> ReadHeaders(SqliteConnection connection)
    {
        List<RelationHeader> headers = [];
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = RelationQuery;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    if (
                        name.StartsWith(InternalPrefix, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(
                            name,
                            VariableStorage.TableName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }

                    headers.Add(
                        new RelationHeader(
                            name,
                            string.Equals(
                                reader.GetString(1),
                                "view",
                                StringComparison.OrdinalIgnoreCase
                            )
                                ? RelationKind.View
                                : RelationKind.Table,
                            reader.IsDBNull(2) ? null : reader.GetString(2),
                            reader.GetInt32(3) == 1
                        )
                    );
                }
            }
        }

        return headers;
    }

    private static List<ColumnSchema> ReadColumns(
        SqliteConnection connection,
        RelationHeader header
    )
    {
        List<RawColumn> raw = [];
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = ColumnQuery;
            command.Parameters.AddWithValue("$name", header.Name);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    raw.Add(
                        new RawColumn(
                            reader.GetString(0),
                            reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            reader.GetInt32(2) != 0,
                            reader.IsDBNull(3) ? null : reader.GetString(3),
                            reader.GetInt32(4)
                        )
                    );
                }
            }
        }

        int primaryKeyColumns = raw.Count((RawColumn column) => column.PrimaryKey > 0);
        List<ColumnSchema> columns = [];
        foreach (RawColumn column in raw)
        {
            bool isPrimaryKey = column.PrimaryKey > 0;
            bool isIdentity =
                header.Kind == RelationKind.Table
                && isPrimaryKey
                && primaryKeyColumns == 1
                && string.Equals(
                    column.Type.Trim(),
                    IntegerType,
                    StringComparison.OrdinalIgnoreCase
                );
            columns.Add(
                new ColumnSchema(
                    column.Name,
                    column.Type,
                    !column.NotNull && !isPrimaryKey,
                    isPrimaryKey,
                    column.PrimaryKey,
                    isIdentity,
                    column.DefaultValue
                )
            );
        }

        return columns;
    }

    private static List<RawForeignKey> ReadForeignKeys(
        SqliteConnection connection,
        RelationHeader header
    )
    {
        Dictionary<int, RawForeignKey> byId = [];
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = ForeignKeyQuery;
            command.Parameters.AddWithValue("$name", header.Name);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    if (!byId.TryGetValue(id, out RawForeignKey? key))
                    {
                        key = new RawForeignKey(
                            reader.GetString(1),
                            reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        );
                        byId[id] = key;
                    }

                    key.Columns.Add(reader.GetString(2));
                    if (!reader.IsDBNull(3))
                    {
                        key.ReferencedColumns.Add(reader.GetString(3));
                    }
                }
            }
        }

        return
        [
            .. byId.OrderBy((KeyValuePair<int, RawForeignKey> pair) => pair.Key)
                .Select((KeyValuePair<int, RawForeignKey> pair) => pair.Value),
        ];
    }

    private static List<ForeignKeySchema> ResolveForeignKeys(
        List<RawForeignKey> keys,
        RelationHeader header
    )
    {
        IReadOnlyDictionary<string, string> names = ForeignKeyNameParser.Parse(header.Sql);
        List<ForeignKeySchema> resolved = [];
        foreach (RawForeignKey key in keys)
        {
            names.TryGetValue(
                ForeignKeyNameParser.BuildKey(key.Columns, key.Table),
                out string? name
            );
            resolved.Add(
                new ForeignKeySchema(
                    name,
                    key.Columns,
                    key.Table,
                    key.ReferencedColumns,
                    key.OnUpdate,
                    key.OnDelete
                )
            );
        }

        return resolved;
    }

    private static List<RelationSchema> ResolveReferencedColumns(List<RelationSchema> relations)
    {
        List<RelationSchema> result = [];
        foreach (RelationSchema relation in relations)
        {
            List<ForeignKeySchema> keys = [];
            foreach (ForeignKeySchema key in relation.ForeignKeys)
            {
                if (key.ReferencedColumns.Count > 0)
                {
                    keys.Add(key);
                    continue;
                }

                RelationSchema? parent = relations.FirstOrDefault(
                    (RelationSchema candidate) =>
                        string.Equals(
                            candidate.Name,
                            key.ReferencedTable,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
                IReadOnlyList<string> primaryKey = parent is null
                    ? []
                    :
                    [
                        .. parent
                            .Columns.Where((ColumnSchema column) => column.IsPrimaryKey)
                            .OrderBy((ColumnSchema column) => column.PrimaryKeyOrdinal)
                            .Select((ColumnSchema column) => column.Name),
                    ];
                keys.Add(key with { ReferencedColumns = primaryKey });
            }

            result.Add(relation with { ForeignKeys = keys });
        }

        return result;
    }

    private static bool IsSystem(string name)
    {
        return SystemViews.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    private sealed record RelationHeader(
        string Name,
        RelationKind Kind,
        string? Sql,
        bool IsTemporary
    );

    private sealed record RawColumn(
        string Name,
        string Type,
        bool NotNull,
        string? DefaultValue,
        int PrimaryKey
    );

    private sealed class RawForeignKey
    {
        public RawForeignKey(string table, string onUpdate, string onDelete)
        {
            Table = table;
            OnUpdate = onUpdate;
            OnDelete = onDelete;
        }

        public string Table { get; }
        public string OnUpdate { get; }
        public string OnDelete { get; }
        public List<string> Columns { get; } = [];
        public List<string> ReferencedColumns { get; } = [];
    }
}

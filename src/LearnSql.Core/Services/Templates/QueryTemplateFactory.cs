using System.Text;

namespace LearnSql.Core.Services.Templates;

public sealed class QueryTemplateFactory : IQueryTemplateFactory
{
    private const int MaxTableTemplates = 3;
    private const int MaxCountedTables = 12;

    private readonly IStringLocalizer<EditorStrings> _text;

    public QueryTemplateFactory(IStringLocalizer<EditorStrings> text)
    {
        _text = text;
    }

    public string CreateStarterQuery(DatabaseSchema schema)
    {
        RelationSchema? first = UserTables(schema).FirstOrDefault();
        return first is null
            ? "SELECT *\nFROM TABLES"
            : $"SELECT TOP 10 *\nFROM {SqlIdentifier.Quote(first.Name)}";
    }

    public IReadOnlyList<QueryTemplate> Create(DatabaseSchema schema)
    {
        List<QueryTemplate> templates =
        [
            new QueryTemplate(
                _text["TemplateAllTables"],
                "SELECT *\nFROM TABLES\nWHERE TABLE_TYPE = 'BASE TABLE'"
            ),
            new QueryTemplate(
                _text["TemplateAllColumns"],
                "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE\nFROM COLUMNS\nORDER BY TABLE_NAME, ORDINAL_POSITION"
            ),
        ];

        List<RelationSchema> tables = [.. UserTables(schema)];
        foreach (RelationSchema table in tables.Take(MaxTableTemplates))
        {
            templates.Add(
                new QueryTemplate(
                    _text["TemplateFirstRows", table.Name],
                    $"SELECT TOP 100 *\nFROM {SqlIdentifier.Quote(table.Name)}"
                )
            );
        }

        QueryTemplate? join = CreateJoinTemplate(tables);
        if (join is not null)
        {
            templates.Add(join);
        }

        if (tables.Count is > 0 and <= MaxCountedTables)
        {
            templates.Add(new QueryTemplate(_text["TemplateRowCounts"], CreateRowCounts(tables)));
        }

        return templates;
    }

    private QueryTemplate? CreateJoinTemplate(IReadOnlyList<RelationSchema> tables)
    {
        foreach (RelationSchema child in tables)
        {
            foreach (ForeignKeySchema key in child.ForeignKeys)
            {
                RelationSchema? parent = tables.FirstOrDefault(
                    (RelationSchema table) =>
                        string.Equals(
                            table.Name,
                            key.ReferencedTable,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
                bool usable =
                    parent is not null
                    && key.Columns.Count > 0
                    && key.Columns.Count == key.ReferencedColumns.Count
                    && !string.Equals(parent.Name, child.Name, StringComparison.OrdinalIgnoreCase);
                if (!usable)
                {
                    continue;
                }

                string condition = string.Join(
                    "\n    AND ",
                    key.Columns.Select(
                        (string column, int index) =>
                            $"c.{SqlIdentifier.Quote(column)} = p.{SqlIdentifier.Quote(key.ReferencedColumns[index])}"
                    )
                );
                string sql =
                    $"SELECT TOP 100 c.*, p.*\nFROM {SqlIdentifier.Quote(child.Name)} AS c\nINNER JOIN {SqlIdentifier.Quote(parent!.Name)} AS p\n    ON {condition}";
                return new QueryTemplate(_text["TemplateJoin", child.Name, parent.Name], sql);
            }
        }

        return null;
    }

    private static string CreateRowCounts(IReadOnlyList<RelationSchema> tables)
    {
        StringBuilder builder = new StringBuilder();
        for (int index = 0; index < tables.Count; index++)
        {
            string name = tables[index].Name;
            if (index > 0)
            {
                builder.Append("\nUNION ALL\n");
            }

            builder.Append(
                $"SELECT '{name.Replace("'", "''", StringComparison.Ordinal)}' AS TableName, COUNT(*) AS RowCount FROM {SqlIdentifier.Quote(name)}"
            );
        }

        return builder.ToString();
    }

    private static IEnumerable<RelationSchema> UserTables(DatabaseSchema schema)
    {
        return schema.Tables.Where((RelationSchema table) => !table.IsSystem && !table.IsTemporary);
    }
}

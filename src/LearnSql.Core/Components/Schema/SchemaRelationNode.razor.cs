using LearnSql.Core.Services.Templates;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Schema;

public partial class SchemaRelationNode
{
    [Parameter, EditorRequired]
    public RelationSchema Relation { get; set; } = default!;

    [Parameter]
    public bool Expanded { get; set; }

    [Parameter]
    public EventCallback<string> OnToggle { get; set; }

    [Parameter]
    public EventCallback<string> OnInsert { get; set; }

    [Inject]
    private IStringLocalizer<SchemaStrings> L { get; set; } = default!;

    private string SelectTop => $"SELECT TOP 100 *\nFROM {Quoted(Relation.Name)}";

    private string RelationIcon =>
        Relation.IsSystem ? Icons.Material.Filled.Settings
        : Relation.Kind == RelationKind.View ? Icons.Material.Filled.Visibility
        : Icons.Material.Filled.TableChart;

    private ForeignKeySchema? ForeignKeyOf(ColumnSchema column)
    {
        return Relation.ForeignKeys.FirstOrDefault(
            (ForeignKeySchema key) =>
                key.Columns.Any(
                    (string name) =>
                        string.Equals(name, column.Name, StringComparison.OrdinalIgnoreCase)
                )
        );
    }

    private string ColumnTooltip(ColumnSchema column, ForeignKeySchema? key)
    {
        string nullability = column.IsNullable ? L["Nullable"] : L["NotNull"];
        string text = $"{column.Name}: {column.DeclaredType}, {nullability}";
        if (column.IsPrimaryKey)
        {
            text += $", {L["PrimaryKey"]}";
        }

        if (key is not null)
        {
            text +=
                $"\n{L["ForeignKeyTarget", key.ReferencedTable, string.Join(", ", key.ReferencedColumns)]}";
        }

        return text;
    }

    private static string Quoted(string name)
    {
        return SqlIdentifier.Quote(name);
    }
}

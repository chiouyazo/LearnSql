using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Schema;

public partial class SchemaBrowser : IDisposable
{
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
    private readonly HashSet<string> _expanded = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    );

    private DatabaseSchema? _schema;
    private IReadOnlyList<SchemaSection> _sections = [];
    private string _filter = string.Empty;
    private bool _loading;
    private bool _failed;

    [Parameter]
    public EventCallback<string> OnInsertText { get; set; }

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IStringLocalizer<SchemaStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<SchemaBrowser> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Session.SchemaChanged += OnSchemaChanged;
        if (Session.IsReady)
        {
            await RefreshAsync();
        }
    }

    public void Dispose()
    {
        Session.SchemaChanged -= OnSchemaChanged;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private bool IsExpanded(RelationSchema relation)
    {
        return _expanded.Contains(relation.Name) || !string.IsNullOrWhiteSpace(_filter);
    }

    private void Toggle(string name)
    {
        if (!_expanded.Remove(name))
        {
            _expanded.Add(name);
        }
    }

    private void OnFilterChanged(string? filter)
    {
        _filter = filter ?? string.Empty;
        BuildSections();
    }

    private void OnSchemaChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        _loading = true;
        _failed = false;
        StateHasChanged();
        try
        {
            _schema = await Session.GetSchemaAsync(_lifetime.Token);
            BuildSections();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Reading the database schema failed.");
            _failed = true;
        }
        finally
        {
            _loading = false;
        }

        StateHasChanged();
    }

    private void BuildSections()
    {
        if (_schema is null)
        {
            _sections = [];
            return;
        }

        List<RelationSchema> visible = [.. _schema.Relations.Where(Matches)];
        List<SchemaSection> sections = [];
        AddSection(
            sections,
            "SectionTables",
            visible,
            (RelationSchema r) => !r.IsSystem && r.Kind == RelationKind.Table
        );
        AddSection(
            sections,
            "SectionViews",
            visible,
            (RelationSchema r) => !r.IsSystem && r.Kind == RelationKind.View
        );
        AddSection(sections, "SectionSystem", visible, (RelationSchema r) => r.IsSystem);
        _sections = sections;
    }

    private static void AddSection(
        List<SchemaSection> sections,
        string titleKey,
        List<RelationSchema> relations,
        Func<RelationSchema, bool> predicate
    )
    {
        List<RelationSchema> selected = [.. relations.Where(predicate)];
        if (selected.Count > 0)
        {
            sections.Add(new SchemaSection(titleKey, selected));
        }
    }

    private bool Matches(RelationSchema relation)
    {
        if (string.IsNullOrWhiteSpace(_filter))
        {
            return !relation.IsTemporary;
        }

        return !relation.IsTemporary
            && (
                Contains(relation.Name)
                || relation.Columns.Any((ColumnSchema column) => Contains(column.Name))
            );
    }

    private bool Contains(string text)
    {
        return text.Contains(_filter.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}

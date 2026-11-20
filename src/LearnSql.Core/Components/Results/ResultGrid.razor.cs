using LearnSql.Core.Services.Results;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Results;

public partial class ResultGrid
{
    private const int IndexColumnRem = 3;
    private const int MinColumnRem = 8;

    private ResultSet? _bound;
    private List<int> _order = [];
    private bool[] _numeric = [];
    private int _sortColumn = -1;
    private ResultSortDirection _direction = ResultSortDirection.Ascending;

    [Parameter, EditorRequired]
    public ResultSet ResultSet { get; set; } = default!;

    [Inject]
    private IResultExporter Exporter { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ResultStrings> L { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<ResultGrid> Logger { get; set; } = default!;

    private string GridStyle =>
        $"--rg-cols:{ResultSet.Columns.Count};--rg-min:{IndexColumnRem + (ResultSet.Columns.Count * MinColumnRem)}rem";

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(_bound, ResultSet))
        {
            return;
        }

        _bound = ResultSet;
        _sortColumn = -1;
        _direction = ResultSortDirection.Ascending;
        _order = [.. Enumerable.Range(0, ResultSet.Rows.Count)];
        _numeric = [.. ResultSet.Columns.Select(IsNumericColumn)];
    }

    private bool IsNumericColumn(ResultColumn column, int index)
    {
        if (column.Kind != ValueKind.Unknown && column.Kind != ValueKind.Null)
        {
            return column.Kind == ValueKind.Numeric;
        }

        object? sample = ResultSet
            .Rows.Select((object?[] row) => row[index])
            .FirstOrDefault((object? value) => value is not null);
        return CellFormatter.IsNumeric(sample);
    }

    private void ToggleSort(int column)
    {
        if (_sortColumn != column)
        {
            _sortColumn = column;
            _direction = ResultSortDirection.Ascending;
        }
        else if (_direction == ResultSortDirection.Ascending)
        {
            _direction = ResultSortDirection.Descending;
        }
        else
        {
            _sortColumn = -1;
            _direction = ResultSortDirection.Ascending;
        }

        _order =
            _sortColumn < 0
                ? [.. Enumerable.Range(0, ResultSet.Rows.Count)]
                : [.. ResultSorter.Sort(ResultSet, _sortColumn, _direction)];
    }

    private string? AriaSort(int column)
    {
        if (_sortColumn != column)
        {
            return "none";
        }

        return _direction == ResultSortDirection.Ascending ? "ascending" : "descending";
    }

    private async Task ExportAsync()
    {
        try
        {
            await Exporter.ExportCsvAsync(ResultSet, _order);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Exporting the result set failed.");
            Snackbar.Add(L["ExportFailed"], Severity.Error);
        }
    }
}

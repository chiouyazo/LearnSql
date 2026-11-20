namespace LearnSql.Core.Services.Results;

public interface IResultExporter
{
    Task ExportCsvAsync(
        ResultSet resultSet,
        IReadOnlyList<int>? rowOrder = null,
        CancellationToken cancellationToken = default
    );
}

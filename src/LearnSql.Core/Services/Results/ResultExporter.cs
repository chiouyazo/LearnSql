using LearnSql.Core.Services.Interop;

namespace LearnSql.Core.Services.Results;

public sealed class ResultExporter : IResultExporter
{
    private const string ContentType = "text/csv;charset=utf-8";

    private readonly IFileDownloader _downloader;
    private readonly TimeProvider _time;

    public ResultExporter(IFileDownloader downloader, TimeProvider time)
    {
        _downloader = downloader;
        _time = time;
    }

    public Task ExportCsvAsync(
        ResultSet resultSet,
        IReadOnlyList<int>? rowOrder = null,
        CancellationToken cancellationToken = default
    )
    {
        string stamp = _time.GetLocalNow().ToString("yyyyMMdd-HHmmss");
        return _downloader.DownloadAsync(
            $"learnsql-result-{stamp}.csv",
            ContentType,
            ResultCsvWriter.ToUtf8Bytes(resultSet, rowOrder),
            cancellationToken
        );
    }
}

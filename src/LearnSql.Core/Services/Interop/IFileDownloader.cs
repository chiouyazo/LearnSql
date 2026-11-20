namespace LearnSql.Core.Services.Interop;

public interface IFileDownloader
{
    Task DownloadAsync(
        string fileName,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken = default
    );
}

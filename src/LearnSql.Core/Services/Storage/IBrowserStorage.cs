namespace LearnSql.Core.Services.Storage;

public interface IBrowserStorage
{
    ValueTask<T?> GetAsync<T>(StorageKey<T> key, CancellationToken cancellationToken = default);

    ValueTask<bool> SetAsync<T>(
        StorageKey<T> key,
        T value,
        CancellationToken cancellationToken = default
    );

    ValueTask<bool> RemoveAsync<T>(
        StorageKey<T> key,
        CancellationToken cancellationToken = default
    );
}

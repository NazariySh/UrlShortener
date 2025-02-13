namespace UrlShortener.Application.Interfaces;

public interface ICacheService
{
    ValueTask<T> GetOrAddAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> fetchFunc,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key);
}
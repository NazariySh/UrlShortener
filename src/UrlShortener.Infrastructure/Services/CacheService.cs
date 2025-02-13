using Microsoft.Extensions.Caching.Hybrid;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly HybridCache _cache;

    public CacheService(HybridCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async ValueTask<T> GetOrAddAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> fetchFunc,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var options = expiration is null
            ? null
            : new HybridCacheEntryOptions { Expiration = expiration };

        return await _cache.GetOrCreateAsync(
            key,
            fetchFunc,
            options,
            cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        await _cache.RemoveAsync(key);
    }
}
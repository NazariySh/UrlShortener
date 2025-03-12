using Microsoft.Extensions.Caching.Hybrid;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Services;

public class HybridCacheService : ICacheService
{
    private readonly HybridCache _cache;

    public HybridCacheService(HybridCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> fetchFunc,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration
        };

        return await _cache.GetOrCreateAsync(
            key,
            fetchFunc,
            options,
            cancellationToken: cancellationToken);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration
        };

        await _cache.SetAsync(
            key,
            value,
            options,
            cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        await _cache.RemoveAsync(key);
    }
}
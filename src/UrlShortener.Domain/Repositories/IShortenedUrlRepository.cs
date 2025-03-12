using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Repositories;

public interface IShortenedUrlRepository : IRepository<ShortenedUrl>
{
    Task AddClickAsync(string uniqueCode, CancellationToken cancellationToken = default);
    Task<bool> IsUniqueCodeAsync(string code, CancellationToken cancellationToken = default);
}
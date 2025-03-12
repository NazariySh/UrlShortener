using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Repositories;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class ShortenedUrlRepository : BaseRepository<ShortenedUrl>, IShortenedUrlRepository
{
    public ShortenedUrlRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task AddClickAsync(string uniqueCode, CancellationToken cancellationToken = default)
    {
        await DbContext.ShortenedUrls
            .Where(x => x.UniqueCode == uniqueCode)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(u => u.Clicks, u => u.Clicks + 1),
                cancellationToken);
    }

    public async Task<bool> IsUniqueCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return !await DbContext.ShortenedUrls
            .AnyAsync(x => x.UniqueCode == code, cancellationToken);
    }
}
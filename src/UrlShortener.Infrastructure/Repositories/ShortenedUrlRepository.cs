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
}
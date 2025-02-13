namespace UrlShortener.Domain.Repositories;

public interface IUnitOfWork
{
    IShortenedUrlRepository ShortenedUrls { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
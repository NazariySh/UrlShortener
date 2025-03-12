namespace UrlShortener.Domain.Repositories;

public interface IUnitOfWork
{
    IShortenedUrlRepository ShortenedUrls { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
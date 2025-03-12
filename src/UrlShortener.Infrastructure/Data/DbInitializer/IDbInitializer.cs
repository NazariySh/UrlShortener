namespace UrlShortener.Infrastructure.Data.DbInitializer;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
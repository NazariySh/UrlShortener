using UrlShortener.Domain.Repositories;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    private IShortenedUrlRepository? _shortenedUrlRepository;
    private IUserRepository? _userRepository;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IShortenedUrlRepository ShortenedUrls =>
        _shortenedUrlRepository ??= new ShortenedUrlRepository(_dbContext);

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
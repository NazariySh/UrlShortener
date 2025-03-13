using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}
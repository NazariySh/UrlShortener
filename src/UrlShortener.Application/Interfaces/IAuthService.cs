using UrlShortener.Application.DTOs.Users;

namespace UrlShortener.Application.Interfaces;

public interface IAuthService
{
    Task LoginAsync(
        LoginDto loginDto,
        CancellationToken cancellationToken = default);
    Task LogoutAsync();
}
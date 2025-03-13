using UrlShortener.Application.DTOs.Users;
using UrlShortener.Domain.Enums;
using UrlShortener.Domain.Models;

namespace UrlShortener.Application.Interfaces;

public interface IUserService
{
    Task CreateAsync(
        RegisterDto registerDto,
        RoleType roleName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PagedList<UserDto>> GetAllAsync(
        ushort pageNumber,
        ushort pageSize,
        CancellationToken cancellationToken = default);
}
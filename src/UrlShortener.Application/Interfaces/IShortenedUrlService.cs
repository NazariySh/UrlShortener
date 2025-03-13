using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Domain.Models;

namespace UrlShortener.Application.Interfaces;

public interface IShortenedUrlService
{
    Task<string> CreateAsync(
        CreateShortenedUrlDto dto,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task<ShortenedUrlDto?> GetAsync(string uniqueCode, CancellationToken cancellationToken = default);

    Task DeleteAsync(string uniqueCode, Guid? userId, CancellationToken cancellationToken = default);

    Task<PagedList<ShortenedUrlDto>> GetAllAsync(
        Guid userId,
        string? search,
        ushort pageNumber,
        ushort pageSize,
        CancellationToken cancellationToken = default);
}
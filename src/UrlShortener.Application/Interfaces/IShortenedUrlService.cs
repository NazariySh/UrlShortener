using UrlShortener.Application.DTOs;

namespace UrlShortener.Application.Interfaces;

public interface IShortenedUrlService
{
    Task<string> CreateAsync(CreateShortenedUrlDto dto, CancellationToken cancellationToken = default);
    Task<ShortenedUrlDto?> GetAsync(string uniqueCode, CancellationToken cancellationToken = default);
}
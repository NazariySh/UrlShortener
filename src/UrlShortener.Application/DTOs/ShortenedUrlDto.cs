namespace UrlShortener.Application.DTOs;

public class ShortenedUrlDto
{
    public Guid Id { get; set; }
    public string LongUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string UniqueCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
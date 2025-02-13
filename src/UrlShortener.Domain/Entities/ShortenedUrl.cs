namespace UrlShortener.Domain.Entities;

public class ShortenedUrl : BaseEntity
{
    public string LongUrl { get; set; } = null!;
    public string ShortUrl { get; set; } = null!;
    public string UniqueCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
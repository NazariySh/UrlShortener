namespace UrlShortener.Domain.Entities;

public class ShortenedUrl : BaseEntity
{
    public string LongUrl { get; set; } = null!;
    public string ShortUrl { get; set; } = null!;
    public string UniqueCode { get; set; } = null!;
    public int Clicks { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
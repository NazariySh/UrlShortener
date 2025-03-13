namespace UrlShortener.WebApp.Areas.User.Models.Home;

public class ShortUrlViewModel
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
}
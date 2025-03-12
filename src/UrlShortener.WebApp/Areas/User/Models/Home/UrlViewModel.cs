using System.ComponentModel.DataAnnotations;

namespace UrlShortener.WebApp.Areas.User.Models.Home;

public class UrlViewModel
{
    [Required]
    [Url]
    public string OriginalUrl { get; set; } = string.Empty;
}
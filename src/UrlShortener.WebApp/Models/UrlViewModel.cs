using System.ComponentModel.DataAnnotations;

namespace UrlShortener.WebApp.Models;

public class UrlViewModel
{
    [Required]
    [Url]
    public string OriginalUrl { get; set; } = string.Empty;
}
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Areas.User.Models.Url;

public class UrlListViewModel
{
    public IReadOnlyList<ShortenedUrlDto> Urls { get; set; } = [];
    public string? Search { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();
}
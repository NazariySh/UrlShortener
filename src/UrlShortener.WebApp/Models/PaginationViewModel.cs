namespace UrlShortener.WebApp.Models;

public class PaginationViewModel
{
    public const int AdjacentPageCount = 3;

    public string Action { get; set; } = "Index";
    public string? Search { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public bool IsFarFromFirstPage => PageNumber > AdjacentPageCount + 1;
    public bool IsFarFromLastPage => PageNumber < TotalPages - AdjacentPageCount;

    public int StartPage => IsFarFromFirstPage ? PageNumber - AdjacentPageCount : 1;
    public int EndPage => IsFarFromLastPage ? PageNumber + AdjacentPageCount : TotalPages;
}
using UrlShortener.Application.DTOs.Users;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Areas.Admin.Models;

public class AccountListViewModel
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
    public PaginationViewModel Pagination { get; set; } = new();
}
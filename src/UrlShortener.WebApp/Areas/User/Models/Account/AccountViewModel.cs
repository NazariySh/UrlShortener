using UrlShortener.Application.DTOs.Users;

namespace UrlShortener.WebApp.Areas.User.Models.Account;

public class AccountViewModel
{
    public UserDto User { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.WebApp.Areas.User.Models.Auth;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
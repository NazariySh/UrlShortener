namespace UrlShortener.Application.DTOs.Users;

public record LoginDto(
    string Email,
    string Password,
    bool RememberMe);
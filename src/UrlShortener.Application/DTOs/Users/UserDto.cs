namespace UrlShortener.Application.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
}
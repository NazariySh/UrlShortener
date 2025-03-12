using System.Security.Authentication;
using System.Security.Claims;
using UrlShortener.Domain.Enums;

namespace UrlShortener.WebApp.Extensions;

public static class ClaimPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal claimsPrincipal)
    {
        var id = GetIdOrDefault(claimsPrincipal);

        if (id is null)
        {
            throw new AuthenticationException("User id not found");
        }

        return id.Value;
    }

    public static Guid? GetIdOrDefault(this ClaimsPrincipal claimsPrincipal)
    {
        var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(id, out var result))
        {
            return result;
        }

        return null;
    }

    public static string GetEmail(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.Email)
            ?? throw new AuthenticationException("User email not found");
    }

    public static bool IsAuthenticated(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Identity?.IsAuthenticated ?? false;
    }

    public static bool IsAdmin(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.IsInRole(nameof(RoleType.Admin));
    }
}
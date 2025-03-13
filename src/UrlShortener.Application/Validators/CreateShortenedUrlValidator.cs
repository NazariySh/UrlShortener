using System.Net;
using FluentValidation;
using System.Text.RegularExpressions;
using UrlShortener.Application.DTOs.ShortenedUrl;

namespace UrlShortener.Application.Validators;

public partial class CreateShortenedUrlValidator : AbstractValidator<CreateShortenedUrlDto>
{
    public const int MaxUrlLength = 2048;

    public CreateShortenedUrlValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty().WithMessage("Original URL is required.")
            .MaximumLength(MaxUrlLength).WithMessage($"Original URL must not exceed {MaxUrlLength} characters.")
            .Matches(GetUrlPattern()).WithMessage("Original URL must start with http:// or https://.")
            .Must(BeValidAbsoluteUrl).WithMessage("Original URL must be a well-formed absolute URL.")
            .Must(NotBeLocalOrPrivate).WithMessage("Original URL must be a public website.")
            .Must(HaveValidTld).WithMessage("Original URL must have a valid domain and TLD.");
    }

    private static bool BeValidAbsoluteUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool NotBeLocalOrPrivate(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        var host = uri.Host;

        if (IPAddress.TryParse(host, out _))
        {
            return !GetPrivateIpPattern().IsMatch(host);
        }

        return !host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
               && !host.Contains("localhost", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HaveValidTld(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        var host = uri.Host;
        return GetTldPattern().IsMatch(host);
    }

    [GeneratedRegex("^https?://", RegexOptions.IgnoreCase)]
    private static partial Regex GetUrlPattern();

    [GeneratedRegex(@"^(127\.0\.0\.1|0\.0\.0\.0|10\.\d+\.\d+\.\d+|172\.(1[6-9]|2\d|3[0-1])\.\d+\.\d+|192\.168\.\d+\.\d+|::1|fe80::|fc00::|fd00::)$", RegexOptions.IgnoreCase)]
    private static partial Regex GetPrivateIpPattern();

    [GeneratedRegex(@"\.[a-zA-Z]{2,}$")]
    private static partial Regex GetTldPattern();
}

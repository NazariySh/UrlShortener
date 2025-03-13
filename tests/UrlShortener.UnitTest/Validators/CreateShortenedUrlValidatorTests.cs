using FluentValidation;
using FluentValidation.TestHelper;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Application.Validators;

namespace UrlShortener.UnitTest.Validators;

public class CreateShortenedUrlValidatorTests
{
    private readonly IValidator<CreateShortenedUrlDto> _validator;

    public CreateShortenedUrlValidatorTests()
    {
        _validator = new CreateShortenedUrlValidator();
    }

    [Fact]
    public void Should_HaveError_WhenUrlIsEmpty()
    {
        var dto = new CreateShortenedUrlDto(string.Empty);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL is required.");
    }

    [Fact]
    public void Should_HaveError_WhenUrlLengthExceedsMax()
    {
        var dto = new CreateShortenedUrlDto(new string('a', CreateShortenedUrlValidator.MaxUrlLength + 1));

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage($"Original URL must not exceed {CreateShortenedUrlValidator.MaxUrlLength} characters.");
    }

    [Fact]
    public void Should_HaveError_WhenUrlStartNotWithHttpOrHttps()
    {
        var dto = new CreateShortenedUrlDto("ftp://example.org");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must start with http:// or https://.");
    }

    [Fact]
    public void Should_HaveError_WhenUrlNotValidAbsoluteUrl()
    {
        var dto = new CreateShortenedUrlDto("https://");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must be a well-formed absolute URL.");
    }

    [Fact]
    public void Should_HaveError_WhenUrlIsLocalOrPrivate()
    {
        var dto = new CreateShortenedUrlDto("http://localhost");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must be a public website.");
    }

    [Fact]
    public void Should_HaveError_WhenUrlWithNotValidTld()
    {
        var dto = new CreateShortenedUrlDto("http://example");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must have a valid domain and TLD.");
    }

    [Fact]
    public void Should_NotHaveError_WhenUrlIsValid()
    {
        var dto = new CreateShortenedUrlDto("https://subdomain.example.org");

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }
}
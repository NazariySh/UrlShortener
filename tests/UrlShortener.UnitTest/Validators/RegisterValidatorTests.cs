using FluentValidation.TestHelper;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Validators;

namespace UrlShortener.UnitTest.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator;

    public RegisterValidatorTests()
    {
        _validator = new RegisterValidator();
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsEmpty()
    {
        var model = new RegisterDto(string.Empty, "Password123");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsInvalid()
    {
        var model = new RegisterDto("invalid-email", "Password123");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Should_NotHaveError_WhenEmailIsValid()
    {
        var model = new RegisterDto("valid.email@example.com", "Password123");

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsEmpty()
    {
        var model = new RegisterDto("email@example.com", string.Empty);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsTooShort()
    {
        var model = new RegisterDto("email@example.com", "short");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters long.");
    }

    [Fact]
    public void Should_HaveError_WhenPasswordLacksUppercase()
    {
        var model = new RegisterDto("email@example.com", "password123");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Should_HaveError_WhenPasswordLacksLowercase()
    {
        var model = new RegisterDto("email@example.com", "PASSWORD123");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Should_HaveError_PasswordLacksNumber()
    {
        var model = new RegisterDto("email@example.com", "Password");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number.");
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsValid()
    {
        var model = new RegisterDto("email@example.com", "Password123");

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
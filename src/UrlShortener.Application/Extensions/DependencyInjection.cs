using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Profiles;
using UrlShortener.Application.Services;
using UrlShortener.Application.Validators;

namespace UrlShortener.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ShortenedUrlProfile).Assembly);

        services.AddSingleton<IValidator<RegisterDto>, RegisterValidator>();
        services.AddSingleton<IValidator<CreateShortenedUrlDto>, CreateShortenedUrlValidator>();

        services.AddSingleton<IUniqueCodeGenerator, UniqueCodeGenerator>();

        services.AddScoped<IShortenedUrlService, ShortenedUrlService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
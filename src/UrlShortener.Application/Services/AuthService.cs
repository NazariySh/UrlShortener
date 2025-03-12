using Microsoft.AspNetCore.Identity;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.Application.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        SignInManager<User> signInManager,
        IUnitOfWork unitOfWork)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        var user = await _unitOfWork.Users.GetAsync(
            x => x.Email != null && x.Email.ToLower() == loginDto.Email.ToLower(),
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with email {loginDto.Email} not found");
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            loginDto.Password,
            loginDto.RememberMe,
            false);

        if (!result.Succeeded)
        {
            throw new ArgumentException("Invalid email or password");
        }
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
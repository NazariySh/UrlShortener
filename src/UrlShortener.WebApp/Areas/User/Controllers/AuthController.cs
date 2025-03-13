using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Enums;
using UrlShortener.Domain.Exceptions;
using UrlShortener.WebApp.Areas.User.Models.Auth;
using UrlShortener.WebApp.Extensions;

namespace UrlShortener.WebApp.Areas.User.Controllers;

[Area("User")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        var register = new RegisterDto(model.Email, model.Password);

        var roleName = RoleType.User;

        if (User.IsAdmin())
        {
            if (Enum.TryParse<RoleType>(model.Role, true, out var role))
            {
                roleName = role;
            }
        }

        await _userService.CreateAsync(register, roleName, cancellationToken);

        TempData["SuccessMessage"] = "User registered successfully.";

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        var loginDto = new LoginDto(model.Email, model.Password, model.RememberMe);

        try
        {
            await _authService.LoginAsync(loginDto, cancellationToken);
        }
        catch (NotFoundException)
        {
            TempData["ErrorMessage"] = "Invalid email or password";
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();

        return RedirectToAction("Index", "Home");
    }
}
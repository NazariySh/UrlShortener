using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.WebApp.Areas.User.Models.Account;
using UrlShortener.WebApp.Extensions;

namespace UrlShortener.WebApp.Areas.User.Controllers;

[Area("User")]
[Authorize]
public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AccountController(
        IUserService userService,
        IAuthService authService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userEmail = User.GetEmail();

        var user = await _userService.GetByEmailAsync(userEmail, cancellationToken)
            ?? throw new AuthenticationException("User not found.");

        return View(new AccountViewModel
        {
            User = user
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        var id = User.GetId();

        await _userService.DeleteAsync(id, cancellationToken);

        await _authService.LogoutAsync();

        TempData["SuccessNotification"] = "My account deleted successfully.";

        return RedirectToAction("Index", "Home");
    }
}
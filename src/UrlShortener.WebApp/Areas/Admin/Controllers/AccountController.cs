using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Enums;
using UrlShortener.WebApp.Areas.Admin.Models;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = nameof(RoleType.Admin))]
public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        ushort pageNumber = 1,
        ushort pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetAllAsync(pageNumber, pageSize, cancellationToken);

        return View(new AccountListViewModel
        {
            Users = users.Items,
            Pagination = new PaginationViewModel
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = users.TotalPages
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);

        TempData["SuccessNotification"] = "User deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.WebApp.Areas.User.Models.Url;
using UrlShortener.WebApp.Extensions;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Areas.User.Controllers;

[Area("User")]
[Authorize]
public class UrlController : Controller
{
    private readonly IShortenedUrlService _shortenedUrlService;

    public UrlController(IShortenedUrlService shortenedUrlService)
    {
        _shortenedUrlService = shortenedUrlService ?? throw new ArgumentNullException(nameof(shortenedUrlService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        ushort pageNumber = 1,
        ushort pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetId();

        var urls = await _shortenedUrlService.GetAllAsync(
            userId,
            search,
            pageNumber,
            pageSize,
            cancellationToken);

        return View(new UrlListViewModel
        {
            Urls = urls.Items,
            Search = search,
            Pagination = new PaginationViewModel
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = urls.TotalPages
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string uniqueCode, CancellationToken cancellationToken)
    {
        var userId = User.GetId();

        await _shortenedUrlService.DeleteAsync(uniqueCode, userId, cancellationToken);

        TempData["SuccessNotification"] = "Shortened URL deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
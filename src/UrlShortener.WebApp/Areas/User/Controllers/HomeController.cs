using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Diagnostics;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Application.Interfaces;
using UrlShortener.WebApp.Areas.User.Models.Home;
using UrlShortener.WebApp.Extensions;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Areas.User.Controllers;

[Area("User")]
public class HomeController : Controller
{
    private readonly IShortenedUrlService _shortenedUrlService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IShortenedUrlService shortenedUrlService,
        ILogger<HomeController> logger)
    {
        _shortenedUrlService = shortenedUrlService ?? throw new ArgumentNullException(nameof(shortenedUrlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult Index()
    {
        return View(new UrlViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(UrlViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.GetIdOrDefault();
        var request = new CreateShortenedUrlDto(model.OriginalUrl);

        var shortenedUrl = await _shortenedUrlService.CreateAsync(
            request,
            userId,
            cancellationToken);

        TempData["SuccessNotification"] = "Shortened new url successfully.";

        return View("ShortUrl", new ShortUrlViewModel
        {
            OriginalUrl = model.OriginalUrl,
            ShortUrl = shortenedUrl
        });
    }

    [HttpGet("{uniqueCode}")]
    public async Task<IActionResult> RedirectToLongUrl(string uniqueCode)
    {
        var urlDto = await _shortenedUrlService.GetAsync(uniqueCode);

        if (urlDto is null)
        {
            return NotFound();
        }

        return Redirect(urlDto.LongUrl);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("An error occurred: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
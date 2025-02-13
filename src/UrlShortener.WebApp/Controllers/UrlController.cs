using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FluentValidation;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Interfaces;
using UrlShortener.WebApp.Models;

namespace UrlShortener.WebApp.Controllers;

public class UrlController : Controller
{
    private readonly IShortenedUrlService _shortenedUrlService;
    private readonly ILogger<UrlController> _logger;

    public UrlController(IShortenedUrlService shortenedUrlService, ILogger<UrlController> logger)
    {
        _shortenedUrlService = shortenedUrlService ?? throw new ArgumentNullException(nameof(shortenedUrlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult Index()
    {
        return View(new UrlViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> ShortenUrl(UrlViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var request = new CreateShortenedUrlDto(model.OriginalUrl);

        try
        {
            var shortenedUrl = await _shortenedUrlService.CreateAsync(request);

            return View("ShortUrl", new ShortUrlViewModel
            {
                OriginalUrl = model.OriginalUrl,
                ShortUrl = shortenedUrl
            });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(nameof(Index), model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while shortening the URL.");

            TempData["ErrorMessage"] = ex.Message;
            return View(nameof(Index), model);
        }
    }


    [HttpGet("{uniqueCode}")]
    public async Task<IActionResult> RedirectToLongUrl(string uniqueCode)
    {
        var urlDto = await _shortenedUrlService.GetAsync(uniqueCode);

        if (urlDto == null)
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
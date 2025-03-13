using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UrlShortener.Domain.Exceptions;
using FluentValidation;

namespace UrlShortener.WebApp.Filters;

public class CustomExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CustomExceptionFilter> _logger;
    private readonly ITempDataDictionaryFactory _tempDataFactory;

    public CustomExceptionFilter(
        ILogger<CustomExceptionFilter> logger,
        ITempDataDictionaryFactory tempDataFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tempDataFactory = tempDataFactory ?? throw new ArgumentNullException(nameof(tempDataFactory));
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var tempData = _tempDataFactory.GetTempData(context.HttpContext);

        _logger.LogError(exception, "An error occurred: {errorMessage}", exception.Message);

        switch (exception)
        {
            case ArgumentException:
            case AlreadyExistsException:
                tempData["ErrorMessage"] = exception.Message;
                context.Result = GetViewResult(context);
                break;

            case AuthenticationException:
                HandleAuthException(context);
                break;

            case ValidationException validationException:
                HandleValidationException(validationException, context);
                break;

            case ForbiddenException:
            case NotFoundException:
                tempData["ErrorNotification"] = exception.Message;
                context.Result = GetViewResult(context);
                break;

            default:
                tempData["ErrorNotification"] = "An error occurred. Please try again later.";
                context.Result = GetViewResult(context);
                break;
        }

        context.ExceptionHandled = true;
    }

    private static void HandleValidationException(ValidationException exception, ExceptionContext context)
    {
        foreach (var error in exception.Errors)
        {
            context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        context.Result = GetViewResult(context);
    }

    private static void HandleAuthException(ExceptionContext context)
    {
        var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        context.Result = new RedirectToActionResult(
            "Login",
            "Auth",
            new { returnUrl });
    }

    private static ViewResult GetViewResult(ExceptionContext context, string? actionName = null)
    {
        var action = context.RouteData.Values["action"]?.ToString() ?? "Error";

        var viewResult = new ViewResult
        {
            ViewName = actionName ?? action,
            ViewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                context.ModelState)
        };

        return viewResult;
    }
}
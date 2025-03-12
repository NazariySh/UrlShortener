using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.Application.Services;

public class ShortenedUrlService : IShortenedUrlService
{
    private const int MaxRetries = 10;

    private readonly AsyncPolicy<string?> _retryPolicy;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueCodeGenerator _uniqueCodeGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateShortenedUrlDto> _validator;
    private readonly ICacheService _cacheService;

    public ShortenedUrlService(
        IUnitOfWork unitOfWork,
        IUniqueCodeGenerator uniqueCodeGenerator,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IValidator<CreateShortenedUrlDto> validator,
        ICacheService cacheService,
        ILogger<ShortenedUrlService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _uniqueCodeGenerator = uniqueCodeGenerator ?? throw new ArgumentNullException(nameof(uniqueCodeGenerator));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

        _retryPolicy = Policy
            .HandleResult<string?>(result => result is null)
            .RetryAsync(
                MaxRetries,
                onRetry: (_, retryCount) =>
                {
                    logger.LogWarning("Failed to generate a unique code. Retrying... Attempt {RetryCount}/{MaxRetries}", retryCount, MaxRetries);
                });
    }

    public async Task<string> CreateAsync(CreateShortenedUrlDto dto, Guid? userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var uniqueCode = await GenerateUniqueCodeAsync(cancellationToken);
        var shortUrl = GetShortUrl(uniqueCode);

        var shortenUrl = new ShortenedUrl
        {
            LongUrl = dto.OriginalUrl,
            ShortUrl = shortUrl,
            UniqueCode = uniqueCode,
            UserId = userId
        };

        _unitOfWork.ShortenedUrls.Add(shortenUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shortUrl;
    }

    public async Task DeleteAsync(
        string uniqueCode,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uniqueCode);

        var shortUrl = await _unitOfWork.ShortenedUrls.GetAsync(
            x => x.UniqueCode == uniqueCode,
            cancellationToken)
            ?? throw new NotFoundException($"Shortened URL with code '{uniqueCode}' not found.");

        if (shortUrl.UserId != userId)
        {
            throw new ForbiddenException($"You do not have permission to delete the shortened URL with code '{uniqueCode}'.");
        }

        _unitOfWork.ShortenedUrls.Remove(shortUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(GetCacheKey(uniqueCode));
    }

    public async Task<ShortenedUrlDto?> GetAsync(string uniqueCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uniqueCode);

        var shortUrl = await _cacheService.GetOrCreateAsync(
            GetCacheKey(uniqueCode),
            async token =>
            {
                return await _unitOfWork.ShortenedUrls.GetAsync(
                    x => x.UniqueCode == uniqueCode,
                    token);
            },
            cancellationToken: cancellationToken);

        if (shortUrl is null)
        {
            return null;
        }

        await _unitOfWork.ShortenedUrls.AddClickAsync(uniqueCode, cancellationToken);

        return _mapper.Map<ShortenedUrlDto>(shortUrl);
    }

    public async Task<PagedList<ShortenedUrlDto>> GetAllAsync(
        Guid userId,
        string? search,
        ushort pageNumber,
        ushort pageSize,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User Id cannot be empty!", nameof(userId));
        }

        Console.WriteLine(search);

        var urls = await _unitOfWork.ShortenedUrls.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            GetSearchFilter(search, userId),
            ascendingSortKeySelector: x => x.CreatedAt,
            cancellationToken: cancellationToken);

        return _mapper.Map<PagedList<ShortenedUrlDto>>(urls);
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        var uniqueCode = await _retryPolicy.ExecuteAsync(async () =>
        {
            var code = _uniqueCodeGenerator.Generate();
            var isUnique = await _unitOfWork.ShortenedUrls.IsUniqueCodeAsync(code, cancellationToken);

            return isUnique ? code : null;
        });

        if (uniqueCode is null)
        {
            throw new InvalidOperationException("Failed to generate a unique code after multiple attempts.");
        }

        return uniqueCode;
    }

    private string GetShortUrl(string uniqueCode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext.Request;

        return $"{request.Scheme}://{request.Host}/{uniqueCode}";
    }

    private static string GetCacheKey(string uniqueCode)
    {
        return $"code-{uniqueCode}";
    }

    private static Expression<Func<ShortenedUrl, bool>> GetSearchFilter(string? search, Guid userId)
    {
        return x =>
            x.UserId == userId &&
            (string.IsNullOrWhiteSpace(search) ||
             x.UniqueCode.Contains(search) ||
             x.ShortUrl.Contains(search) ||
             x.LongUrl.Contains(search));
    }
}
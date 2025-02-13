using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.Application.Services;

public class ShortenedUrlService : IShortenedUrlService
{
    private const int MaxAttempts = 10;

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
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _uniqueCodeGenerator = uniqueCodeGenerator ?? throw new ArgumentNullException(nameof(uniqueCodeGenerator));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<string> CreateAsync(CreateShortenedUrlDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var uniqueCode = await GenerateUniqueCodeAsync(cancellationToken);
        var shortUrl = GetShortUrl(uniqueCode);

        var shortenUrl = new ShortenedUrl
        {
            LongUrl = dto.OriginalUrl,
            ShortUrl = shortUrl,
            UniqueCode = uniqueCode
        };

        _unitOfWork.ShortenedUrls.Add(shortenUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shortUrl;
    }

    public async Task<ShortenedUrlDto?> GetAsync(string uniqueCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uniqueCode);

        var cacheKey = $"code-{uniqueCode}";

        var shortUrl = await _cacheService.GetOrAddAsync(
            cacheKey,
            async token =>
            {
                return await _unitOfWork.ShortenedUrls.GetAsync(
                    x => x.UniqueCode == uniqueCode,
                    token);
            },
            cancellationToken: cancellationToken);

        return _mapper.Map<ShortenedUrlDto>(shortUrl);
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var uniqueCode = _uniqueCodeGenerator.Generate();

            if (!await _unitOfWork.ShortenedUrls.AnyAsync(x => x.UniqueCode == uniqueCode, cancellationToken))
            {
                return uniqueCode;
            }
        }

        throw new InvalidOperationException("Failed to generate a unique code after multiple attempts.");
    }

    private string GetShortUrl(string uniqueCode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext.Request;

        return $"{request.Scheme}://{request.Host}/{uniqueCode}";
    }
}
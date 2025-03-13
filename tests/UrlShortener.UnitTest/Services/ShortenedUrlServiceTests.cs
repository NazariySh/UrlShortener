using System.Linq.Expressions;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.UnitTest.Services;

public class ShortenedUrlServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUniqueCodeGenerator> _mockUniqueCodeGenerator;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IValidator<CreateShortenedUrlDto>> _mockValidator;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<ShortenedUrlService>> _mockLogger;
    private readonly ShortenedUrlService _shortenedUrlService;

    public ShortenedUrlServiceTests()
    {
        _fixture = new Fixture();

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUniqueCodeGenerator = new Mock<IUniqueCodeGenerator>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IValidator<CreateShortenedUrlDto>>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ShortenedUrlService>>();

        _shortenedUrlService = new ShortenedUrlService(
            _mockUnitOfWork.Object,
            _mockUniqueCodeGenerator.Object,
            _mockHttpContextAccessor.Object,
            _mockMapper.Object,
            _mockValidator.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowValidationException_WhenValidationFails()
    {
        var userId = _fixture.Create<Guid>();
        var createDto = _fixture.Create<CreateShortenedUrlDto>();

        SetupMockValidatorThrowsValidationException();

        var act = async () => await _shortenedUrlService.CreateAsync(createDto, userId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowInvalidOperationException_WhenMaxRetriesReached()
    {
        var uniqueCode = _fixture.Create<string>();
        var userId = _fixture.Create<Guid>();
        var createDto = _fixture.Create<CreateShortenedUrlDto>();

        SetupMockUniqueCodeGenerator(uniqueCode);
        SetupMockRepositoryIsUniqueCode(false);

        var act = async () => await _shortenedUrlService.CreateAsync(createDto, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to generate a unique code after multiple attempts.");

        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(10));
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnShortUrl_WhenValidRequest()
    {
        const string requestScheme = "https";
        const string requestHost = "localhost";

        var uniqueCode = _fixture.Create<string>();
        var shortenedUrl = GetShortenedUrl(uniqueCode, $"{requestScheme}://{requestHost}/{uniqueCode}");
        var userId = shortenedUrl.UserId;
        var createDto = _fixture.Build<CreateShortenedUrlDto>()
            .With(x => x.OriginalUrl, shortenedUrl.LongUrl)
            .Create();

        SetupMockUniqueCodeGenerator(uniqueCode);
        SetupMockRepositoryIsUniqueCode(true);
        SetupMockHttpContextAccessorRequest(requestScheme, requestHost);
        SetupMockRepositoryAdd(shortenedUrl);
        SetupMockUnitOfWorkSaveChangesReturns(1);

        var result = await _shortenedUrlService.CreateAsync(createDto, userId);

        result.Should().Be(shortenedUrl.ShortUrl);
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowNotFoundException_WhenShortenedUrlDoesNotExists()
    {
        var userId = _fixture.Create<Guid>();
        var uniqueCode = _fixture.Create<string>();

        SetupMockRepositoryGet(null);

        var act = async () => await _shortenedUrlService.DeleteAsync(uniqueCode, userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowForbiddenException_WhenUserDoesNotOwnUrl()
    {
        var nonOwnerUserId = _fixture.Create<Guid>();
        var shortenedUrl = GetShortenedUrl();
        var uniqueCode = shortenedUrl.UniqueCode;

        SetupMockRepositoryGet(shortenedUrl);

        var act = async () => await _shortenedUrlService.DeleteAsync(uniqueCode, nonOwnerUserId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteShortenedUrl_WhenValidRequest()
    {
        var shortenedUrl = GetShortenedUrl();
        var userId = shortenedUrl.UserId;
        var uniqueCode = shortenedUrl.UniqueCode;

        SetupMockRepositoryGet(shortenedUrl);
        SetupMockUnitOfWorkSaveChangesReturns(1);

        await _shortenedUrlService.DeleteAsync(uniqueCode, userId);

        _mockUnitOfWork.Verify(
            u => u.ShortenedUrls.Remove(shortenedUrl),
            Times.Once);

        _mockCacheService.Verify(
            c => c.RemoveAsync($"code-{uniqueCode}"),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenUrlDoesNotExists()
    {
        var uniqueCode = _fixture.Create<string>();

        SetupMockCacheServiceGetOrCreate(null);

        var result = await _shortenedUrlService.GetAsync(uniqueCode);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_Should_AddClick_WhenUrlExists()
    {
        var shortenedUrl = GetShortenedUrl();
        var shortenedUrlDto = GetShortenedUrlDto(shortenedUrl);
        var uniqueCode = shortenedUrl.UniqueCode;

        SetupMockRepositoryGet(shortenedUrl);
        SetupMockCacheServiceGetOrCreate(shortenedUrl);
        SetupMockMapper(shortenedUrlDto);

        await _shortenedUrlService.GetAsync(uniqueCode);

        _mockUnitOfWork.Verify(
            u => u.ShortenedUrls.AddClickAsync(uniqueCode, default),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnShortenedUrlDto_WhenUrlExists()
    {
        var shortenedUrl = GetShortenedUrl();
        var shortenedUrlDto = GetShortenedUrlDto(shortenedUrl);
        var uniqueCode = shortenedUrl.UniqueCode;

        SetupMockRepositoryGet(shortenedUrl);
        SetupMockCacheServiceGetOrCreate(shortenedUrl);
        SetupMockMapper(shortenedUrlDto);

        var result = await _shortenedUrlService.GetAsync(uniqueCode);

        result.Should().BeEquivalentTo(shortenedUrlDto);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnEmptyList_WhenNoUrlsExists()
    {
        const ushort pageNumber = 1;
        const ushort pageSize = 10;

        var userId = _fixture.Create<Guid>();
        var urls = new PagedList<ShortenedUrl>([], 0, pageNumber, pageSize);
        var urlsDto = new PagedList<ShortenedUrlDto>([], 0, pageNumber, pageSize);

        SetupMockRepositoryGetAllPaginated(urls);
        SetupMockMapper(urlsDto);

        var result = await _shortenedUrlService.GetAllAsync(userId, null, pageNumber, pageSize);

        result.Should().BeEquivalentTo(urlsDto);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnPagedListOfUrl_WhenUrlsExists()
    {
        const ushort pageNumber = 1;
        const ushort pageSize = 10;

        var userId = _fixture.Create<Guid>();
        var urls = _fixture.Create<PagedList<ShortenedUrl>>();
        var urlsDto = GetPaginationResponse(urls);

        SetupMockRepositoryGetAllPaginated(urls);
        SetupMockMapper(urlsDto);

        var result = await _shortenedUrlService.GetAllAsync(userId, null, pageNumber, pageSize);

        result.Items.Count.Should().Be(urls.Items.Count);
        result.Should().BeEquivalentTo(urlsDto);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnPagedListOfUrls_WhenUrlsExist_And_FilterByUniqueCode()
    {
        const ushort pageNumber = 1;
        const ushort pageSize = 10;

        var userId = _fixture.Create<Guid>();
        var urls = _fixture.Create<PagedList<ShortenedUrl>>();
        var urlsDto = GetPaginationResponse(urls);
        var search = urls.Items.First().UniqueCode;

        SetupMockRepositoryGetAllPaginated(urls);
        SetupMockMapper(urlsDto);

        var result = await _shortenedUrlService.GetAllAsync(userId, search, pageNumber, pageSize);

        result.Items.Count.Should().Be(urls.Items.Count);
        result.Should().BeEquivalentTo(urlsDto);
    }

    private static PagedList<ShortenedUrlDto> GetPaginationResponse(PagedList<ShortenedUrl> urls)
    {
        var urlsDto = urls.Items.Select(GetShortenedUrlDto).ToList();

        return new PagedList<ShortenedUrlDto>(
            urlsDto,
            urls.TotalCount,
            urls.PageNumber,
            urls.PageSize);
    }

    private ShortenedUrl GetShortenedUrl(string? uniqueCode = null, string? shortenedUrl = null)
    {
        var code = uniqueCode ?? _fixture.Create<string>();

        return _fixture.Build<ShortenedUrl>()
            .With(x => x.UniqueCode, code)
            .With(x => x.UserId, _fixture.Create<Guid>())
            .With(x => x.ShortUrl, shortenedUrl ?? $"https://localhost/{uniqueCode}")
            .Create();
    }

    private static ShortenedUrlDto GetShortenedUrlDto(ShortenedUrl url)
    {
        return new ShortenedUrlDto
        {
            Id = url.Id,
            LongUrl = url.LongUrl,
            ShortUrl = url.ShortUrl,
            UniqueCode = url.UniqueCode,
            Clicks = url.Clicks,
            CreatedAt = url.CreatedAt
        };
    }

    private void SetupMockValidatorThrowsValidationException()
    {
        _mockValidator.Setup(p => p.ValidateAsync(
                It.Is<ValidationContext<CreateShortenedUrlDto>>(context => context.ThrowOnFailures),
                default))
            .Throws(new ValidationException("error"));
    }

    private void SetupMockUniqueCodeGenerator(string uniqueCode)
    {
        _mockUniqueCodeGenerator.Setup(u => u.Generate())
            .Returns(uniqueCode);
    }

    private void SetupMockHttpContextAccessorRequest(string requestScheme, string requestHost)
    {
        _mockHttpContextAccessor.Setup(h => h.HttpContext.Request.Scheme)
            .Returns(requestScheme);

        _mockHttpContextAccessor.Setup(h => h.HttpContext.Request.Host)
            .Returns(new HostString(requestHost));
    }

    private void SetupMockRepositoryAdd(ShortenedUrl shortenedUrl)
    {
        _mockUnitOfWork.Setup(u => u.ShortenedUrls.Add(It.IsAny<ShortenedUrl>()))
            .Returns(shortenedUrl);
    }

    private void SetupMockRepositoryGet(ShortenedUrl? shortenedUrl)
    {
        _mockUnitOfWork.Setup(u => u.ShortenedUrls.GetAsync(
                It.IsAny<Expression<Func<ShortenedUrl, bool>>>(),
                default))
            .ReturnsAsync(shortenedUrl);
    }

    private void SetupMockRepositoryGetAllPaginated(PagedList<ShortenedUrl> urls)
    {
        _mockUnitOfWork
            .Setup(x => x.ShortenedUrls.GetAllPaginatedAsync(
                It.IsAny<ushort>(),
                It.IsAny<ushort>(),
                It.IsAny<Expression<Func<ShortenedUrl, bool>>>(),
                default,
                default,
                It.IsAny<Expression<Func<ShortenedUrl, object>>>(),
                default,
                default))
            .ReturnsAsync(urls);
    }

    private void SetupMockRepositoryIsUniqueCode(bool result)
    {
        _mockUnitOfWork.Setup(u => u.ShortenedUrls.IsUniqueCodeAsync(
                It.IsAny<string>(),
                default))
            .ReturnsAsync(result);
    }

    private void SetupMockUnitOfWorkSaveChangesReturns(int number)
    {
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(number);
    }

    private void SetupMockMapper(ShortenedUrlDto urlDto)
    {
        _mockMapper
            .Setup(x => x.Map<ShortenedUrlDto>(
                It.IsAny<ShortenedUrl>()))
            .Returns(urlDto);
    }

    private void SetupMockMapper(PagedList<ShortenedUrlDto> data)
    {
        _mockMapper
            .Setup(x => x.Map<PagedList<ShortenedUrlDto>>(
                It.IsAny<PagedList<ShortenedUrl>>()))
            .Returns(data);
    }

    private void SetupMockCacheServiceGetOrCreate(ShortenedUrl? shortenedUrl)
    {
        _mockCacheService.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<ShortenedUrl?>>>(),
                default,
                default))
            .ReturnsAsync(shortenedUrl);
    }
}
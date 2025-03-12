using System.Linq.Expressions;
using System.Security.Authentication;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.UnitTest.Services;

public class AuthServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _fixture = new Fixture();

        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<User>>(
            mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>());

        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _authService = new AuthService(
            _mockSignInManager.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task LoginAsync_Should_ThrowNotFoundException_WhenUserDoesNotExists()
    {
        var loginDto = _fixture.Create<LoginDto>();

        SetupMockRepositoryGet(null);

        var act = async () => await _authService.LoginAsync(loginDto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task LoginAsync_Should_ThrowArgumentException_WhenInvalidPassword()
    {
        var user = _fixture.Create<User>();
        var loginDto = _fixture.Build<LoginDto>()
            .With(x => x.Email, user.Email)
            .Create();

        SetupMockRepositoryGet(user);
        SetupMockSignInManagerPasswordSignIn(false);

        var act = async () => await _authService.LoginAsync(loginDto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_Should_CompleteSuccessfully_WhenValidCredentials()
    {
        var user = _fixture.Create<User>();
        var loginDto = _fixture.Build<LoginDto>()
            .With(x => x.Email, user.Email)
            .Create();

        SetupMockRepositoryGet(user);
        SetupMockSignInManagerPasswordSignIn(true);

        var act = async () => await _authService.LoginAsync(loginDto);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogoutAsync_Should_CallSignOutAsync()
    {
        await _authService.LogoutAsync();

        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    private void SetupMockRepositoryGet(User? user)
    {
        _mockUnitOfWork.Setup(u => u.Users.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                default))
            .ReturnsAsync(user);
    }

    private void SetupMockSignInManagerPasswordSignIn(bool isSuccess)
    {
        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                false))
            .ReturnsAsync(isSuccess ? SignInResult.Success : SignInResult.Failed);
    }
}
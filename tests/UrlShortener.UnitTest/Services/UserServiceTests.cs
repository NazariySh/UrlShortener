using System.Linq.Expressions;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Moq;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.UnitTest.Services;

public class UserServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IValidator<RegisterDto>> _mockValidator;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _fixture = new Fixture();

        _mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IValidator<RegisterDto>>();

        _userService = new UserService(
            _mockUserManager.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowValidationException_WhenValidationFails()
    {
        var registerDto = _fixture.Create<RegisterDto>();

        SetupMockValidatorThrowsValidationException();

        var act = async () => await _userService.CreateAsync(registerDto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowAlreadyExistsException_WhenEmailIsNotUnique()
    {
        var registerDto = _fixture.Create<RegisterDto>();

        SetupMockRepositoryIsEmailUnique(false);

        var act = async () => await _userService.CreateAsync(registerDto);

        await act.Should().ThrowAsync<AlreadyExistsException>();
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowInvalidOperationException_WhenCreateFails()
    {
        var user = _fixture.Create<User>();
        var registerDto = _fixture.Build<RegisterDto>()
            .With(x => x.Email, user.Email)
            .Create();

        SetupMockRepositoryIsEmailUnique(true);
        SetupMockMapper(user);
        SetupMockUserManagerCreate(false);

        var act = async () => await _userService.CreateAsync(registerDto);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowInvalidOperationException_WhenAddToRoleFails()
    {
        var user = _fixture.Create<User>();
        var registerDto = _fixture.Build<RegisterDto>()
            .With(x => x.Email, user.Email)
            .Create();

        SetupMockRepositoryIsEmailUnique(true);
        SetupMockMapper(user);
        SetupMockUserManagerCreate(true);
        SetupMockUserManagerAddToRole(false);

        var act = async () => await _userService.CreateAsync(registerDto);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_Should_NotThrowException_WhenAllSucceed()
    {
        var user = _fixture.Create<User>();
        var registerDto = _fixture.Build<RegisterDto>()
            .With(x => x.Email, user.Email)
            .Create();

        SetupMockRepositoryIsEmailUnique(true);
        SetupMockMapper(user);
        SetupMockUserManagerCreate(true);
        SetupMockUserManagerAddToRole(true);

        var act = async () => await _userService.CreateAsync(registerDto);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowNotFoundException_WhenUserDoesNotExists()
    {
        var userId = _fixture.Create<Guid>();

        SetupMockRepositoryGet(null);

        var act = async () => await _userService.DeleteAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowInvalidOperationException_WhenDeleteFails()
    {
        var user = _fixture.Create<User>();
        var userId = user.Id;

        SetupMockRepositoryGet(user);
        SetupMockUserManagerDelete(false);

        var act = async () => await _userService.DeleteAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_NotThrowException_WhenAllSucceed()
    {
        var user = _fixture.Create<User>();
        var userId = user.Id;

        SetupMockRepositoryGet(user);
        SetupMockUserManagerDelete(true);

        var act = async () => await _userService.DeleteAsync(userId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnNull_WhenUserDoesNotExists()
    {
        var email = _fixture.Create<string>();

        SetupMockRepositoryGet(null);

        var result = await _userService.GetByEmailAsync(email);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnUser_WhenUserExists()
    {
        var user = _fixture.Create<User>();
        var userDto = GetUserDto(user);

        SetupMockRepositoryGet(user);
        SetupMockMapper(userDto);
        SetupMockUserManagerGetRoles();

        var result = await _userService.GetByEmailAsync(userDto.Email);

        result.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnEmptyList_WhenNoUsersExists()
    {
        const ushort pageNumber = 1;
        const ushort pageSize = 10;

        var users = new PagedList<User>([], 0, pageNumber, pageSize);
        var usersDto = new PagedList<UserDto>([], 0, pageNumber, pageSize);

        SetupMockRepositoryGetAllPaginated(users);
        SetupMockMapper(usersDto);
        SetupMockUserManagerGetRoles();

        var result = await _userService.GetAllAsync(pageNumber, pageSize);

        result.Items.Count.Should().Be(0);
        result.Should().BeEquivalentTo(usersDto);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnPagedListOfUsers_WhenUsersExists()
    {
        const ushort pageNumber = 1;
        const ushort pageSize = 10;

        var users = _fixture.Create<PagedList<User>>();
        var usersDto = GetPaginationResponse(users);

        SetupMockRepositoryGetAllPaginated(users);
        SetupMockMapper(usersDto);
        SetupMockUserManagerGetRoles();

        var result = await _userService.GetAllAsync(pageNumber, pageSize);

        result.Items.Count.Should().Be(users.Items.Count);
        result.Should().BeEquivalentTo(usersDto);
    }

    private static PagedList<UserDto> GetPaginationResponse(PagedList<User> users)
    {
        var usersDto = users.Items.Select(GetUserDto).ToList();

        return new PagedList<UserDto>(
            usersDto,
            users.TotalCount,
            users.PageNumber,
            users.PageSize);
    }

    private static UserDto GetUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
        };
    }

    private void SetupMockValidatorThrowsValidationException()
    {
        _mockValidator.Setup(p => p.ValidateAsync(
                It.Is<ValidationContext<RegisterDto>>(context => context.ThrowOnFailures),
                default))
            .Throws(new ValidationException("error"));
    }

    private void SetupMockRepositoryIsEmailUnique(bool result)
    {
        _mockUnitOfWork
            .Setup(x => x.Users.IsEmailUniqueAsync(
                It.IsAny<string>(),
                default))
            .ReturnsAsync(result);
    }

    private void SetupMockRepositoryGet(User? user)
    {
        _mockUnitOfWork
            .Setup(x => x.Users.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                default))
            .ReturnsAsync(user);
    }

    private void SetupMockRepositoryGetAllPaginated(PagedList<User> users)
    {
        _mockUnitOfWork
            .Setup(x => x.Users.GetAllPaginatedAsync(
                It.IsAny<ushort>(),
                It.IsAny<ushort>(),
                default,
                default,
                default,
                default,
                default,
                default))
            .ReturnsAsync(users);
    }

    private void SetupMockMapper(User user)
    {
        _mockMapper
            .Setup(x => x.Map<User>(
                It.IsAny<RegisterDto>()))
            .Returns(user);
    }

    private void SetupMockMapper(UserDto userDto)
    {
        _mockMapper
            .Setup(x => x.Map<UserDto>(
                It.IsAny<User>()))
            .Returns(userDto);
    }

    private void SetupMockMapper(PagedList<UserDto> data)
    {
        _mockMapper
            .Setup(x => x.Map<PagedList<UserDto>>(
                It.IsAny<PagedList<User>>()))
            .Returns(data);
    }

    private void SetupMockUserManagerCreate(bool isSuccess)
    {
        _mockUserManager
            .Setup(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()))
            .ReturnsAsync(isSuccess ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerDelete(bool isSuccess)
    {
        _mockUserManager
            .Setup(x => x.DeleteAsync(
                It.IsAny<User>()))
            .ReturnsAsync(isSuccess ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerAddToRole(bool isSuccess)
    {
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(
                It.IsAny<User>(),
                It.IsAny<string>()))
            .ReturnsAsync(isSuccess ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerGetRoles()
    {
        _mockUserManager
            .Setup(x => x.GetRolesAsync(
                It.IsAny<User>()))
            .ReturnsAsync([]);
    }
}
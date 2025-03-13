using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Enums;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Repositories;

namespace UrlShortener.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterDto> _validator;

    public UserService(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<RegisterDto> validator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task CreateAsync(
        RegisterDto registerDto,
        RoleType roleName = RoleType.User,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registerDto);

        await _validator.ValidateAndThrowAsync(registerDto, cancellationToken);

        if (!await _unitOfWork.Users.IsEmailUniqueAsync(registerDto.Email, cancellationToken))
        {
            throw new AlreadyExistsException($"User with email {registerDto.Email} already exists");
        }

        var user = _mapper.Map<User>(registerDto);

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleName.ToString());

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add role '{roleName.ToString()}'");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        var user = await _unitOfWork.Users.GetAsync(
            x => x.Id == id,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id {id} not found");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to delete user");
        }
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        var user = await _unitOfWork.Users.GetAsync(
            x => x.Email != null && x.Email.ToLower() == email.ToLower(),
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = await _userManager.GetRolesAsync(user);

        return userDto;
    }

    public async Task<PagedList<UserDto>> GetAllAsync(
        ushort pageNumber,
        ushort pageSize,
        CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            cancellationToken: cancellationToken);

        var usersDto = _mapper.Map<PagedList<UserDto>>(users);

        var usersDtoDictionary = usersDto.Items.ToDictionary(x => x.Id, x => x);

        foreach (var user in users.Items)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            if (usersDtoDictionary.TryGetValue(user.Id, out var userDto))
            {
                userDto.Roles = userRoles;
            }
        }

        return usersDto;
    }
}
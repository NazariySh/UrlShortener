using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Enums;

namespace UrlShortener.Infrastructure.Data.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        ApplicationDbContext dbContext,
        IUserService userService,
        ILogger<DbInitializer> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await ApplyMigrationsAsync(cancellationToken);

        await SeedAdminAsync();
    }

    private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var migrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

            if (migrations.Any())
            {
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }

    private async Task SeedAdminAsync()
    {
        const string email = "admin@gmail.com";

        if (_dbContext.Users.Any(x => x.UserName == email))
        {
            return;
        }

        const string password = "Admin123*";

        var registerDto = new RegisterDto(email, password);

        await _userService.CreateAsync(registerDto, RoleType.Admin);
    }
}
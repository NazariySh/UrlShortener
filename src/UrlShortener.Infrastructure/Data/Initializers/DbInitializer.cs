using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Enums;
using UrlShortener.Domain.Settings;

namespace UrlShortener.Infrastructure.Data.Initializers;

public class DbInitializer : IDbInitializer
{
    private readonly AdminSettings _adminSettings;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        IOptions<AdminSettings> adminSettings,
        ApplicationDbContext dbContext,
        IUserService userService,
        ILogger<DbInitializer> logger)
    {
        _adminSettings = adminSettings.Value ?? throw new ArgumentNullException(nameof(adminSettings));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await ApplyMigrationsAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
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

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(x => x.UserName == _adminSettings.Email, cancellationToken))
        {
            return;
        }

        var registerDto = new RegisterDto(_adminSettings.Email, _adminSettings.Password);

        try
        {
            await _userService.CreateAsync(registerDto, RoleType.Admin, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the admin user.");
            throw;
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Infrastructure.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        var roles = new List<IdentityRole<Guid>>
        {
            new() { Id = Guid.Parse("f4a3d9b2-28b9-4670-b7f5-07b6e5cf107f"), Name = "Admin", NormalizedName = "ADMIN" },
            new() { Id = Guid.Parse("a1b9c8f0-1d2e-43b2-b3d2-3c30b354697d"), Name = "User", NormalizedName = "USER" }
        };

        builder.HasData(roles);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Application.Services;
using UrlShortener.Application.Validators;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Configurations;

internal sealed class ShortenedUrlConfiguration : IEntityTypeConfiguration<ShortenedUrl>
{
    public void Configure(EntityTypeBuilder<ShortenedUrl> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LongUrl)
            .HasMaxLength(CreateShortenedUrlValidator.MaxUrlLength)
            .IsRequired();

        builder.Property(x => x.ShortUrl)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.UniqueCode)
            .HasMaxLength(UniqueCodeGenerator.UniqueCodeLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.UniqueCode).IsUnique();
    }
}
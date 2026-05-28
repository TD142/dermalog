using Dermalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dermalog.Api.Data.Configurations;

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("photos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ObjectKey).IsRequired().HasMaxLength(512);

        builder.Property(p => p.ContentType).IsRequired().HasMaxLength(100);

        builder.Property(p => p.CapturedAt).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(p => p.ObjectKey).IsUnique();

        builder.HasIndex(p => p.CapturedAt).IsDescending();
    }
}

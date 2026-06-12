using System.Text.Json;
using Dermalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dermalog.Api.Data.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Text).IsRequired().HasMaxLength(4000);

        builder.Property(e => e.Summary).IsRequired().HasMaxLength(1000);

        builder.Property(e => e.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(e => e.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v =>
                    JsonSerializer.Deserialize<JournalTags>(v, JsonOptions)
                    ?? new JournalTags(
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<string>()
                    ),
                new ValueComparer<JournalTags>(
                    (a, b) =>
                        JsonSerializer.Serialize(a, JsonOptions)
                        == JsonSerializer.Serialize(b, JsonOptions),
                    v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                    v => v
                )
            );

        builder.HasIndex(e => e.CreatedAt).IsDescending();
    }
}

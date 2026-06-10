using System.Text.Json;
using Dermalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dermalog.Api.Data.Configurations;

public class ComparisonConfiguration : IEntityTypeConfiguration<Comparison>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Comparison> builder)
    {
        builder.ToTable("comparisons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.OverallSummary).IsRequired().HasMaxLength(2000);

        builder
            .Property(c => c.SeverityTrend)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.GeneratedAt).IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(c => c.Observations)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v =>
                    JsonSerializer.Deserialize<List<ComparisonObservation>>(v, JsonOptions)
                    ?? new List<ComparisonObservation>(),
                new ValueComparer<IReadOnlyList<ComparisonObservation>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (acc, o) => HashCode.Combine(acc, o.GetHashCode())),
                    v => v.ToList()
                )
            );

        builder
            .HasOne<Photo>()
            .WithMany()
            .HasForeignKey(c => c.BeforePhotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Photo>()
            .WithMany()
            .HasForeignKey(c => c.AfterPhotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.GeneratedAt).IsDescending();
    }
}

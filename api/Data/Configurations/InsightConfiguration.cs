using Dermalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dermalog.Api.Data.Configurations;

public class InsightConfiguration : IEntityTypeConfiguration<Insight>
{
    public void Configure(EntityTypeBuilder<Insight> builder)
    {
        builder.ToTable("insights");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Headline).IsRequired().HasMaxLength(200);

        builder.Property(i => i.Body).IsRequired().HasMaxLength(2000);

        builder.Property(i => i.BasisComparisonCount).IsRequired();

        builder.Property(i => i.BasisLatestComparisonAt).IsRequired();

        builder.Property(i => i.GeneratedAt).IsRequired();
    }
}

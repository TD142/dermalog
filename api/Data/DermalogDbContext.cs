using Dermalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dermalog.Api.Data;

public class DermalogDbContext(DbContextOptions<DermalogDbContext> options) : DbContext(options)
{
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Comparison> Comparisons => Set<Comparison>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DermalogDbContext).Assembly);
    }
}

using Amazon.S3;
using Dermalog.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Testcontainers.PostgreSql;

namespace Dermalog.Api.Tests.Infrastructure;

public class DermalogAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("dermalog_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Mock<IAmazonS3> S3Mock { get; } = new();

    public Task InitializeAsync() => _postgres.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _postgres.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<DermalogDbContext>>();
            services.AddDbContext<DermalogDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString())
            );

            services.RemoveAll<IAmazonS3>();
            services.AddSingleton(S3Mock.Object);
        });
    }
}

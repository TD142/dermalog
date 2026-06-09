using Amazon.S3;
using Amazon.S3.Model;
using Dermalog.Api.Data;
using Dermalog.Api.Infrastructure.Bedrock;
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
    public Mock<IBedrockClient> BedrockMock { get; } = new();

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

            services.RemoveAll<IBedrockClient>();
            services.AddSingleton(BedrockMock.Object);
        });
    }

    /// <summary>
    /// Configure the S3 mock to return the given bytes when GetObjectAsync is called for objectKey.
    /// </summary>
    public void SetupS3Object(string objectKey, byte[] bytes)
    {
        S3Mock
            .Setup(s =>
                s.GetObjectAsync(It.IsAny<string>(), objectKey, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => new GetObjectResponse { ResponseStream = new MemoryStream(bytes) });
    }
}

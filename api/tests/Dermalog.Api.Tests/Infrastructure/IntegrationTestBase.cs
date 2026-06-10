using Dermalog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dermalog.Api.Tests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<DermalogAppFactory>, IAsyncLifetime
{
    protected readonly DermalogAppFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(DermalogAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();
        await db.Database.MigrateAsync();
        db.Comparisons.RemoveRange(db.Comparisons);
        db.Photos.RemoveRange(db.Photos);
        await db.SaveChangesAsync();
        Factory.S3Mock.Invocations.Clear();
        Factory.BedrockMock.Invocations.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

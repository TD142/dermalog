using System.Net;
using Amazon.S3.Model;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class DeletePhotoTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Given_Photo_When_Delete_Then_RemovesRowAndDeletesObject()
    {
        SetupS3Delete();
        var id = await SeedPhoto();

        var response = await Client.DeleteAsync($"/api/v1/photos/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();
        (await db.Photos.AnyAsync(p => p.Id == id)).Should().BeFalse();

        Factory.S3Mock.Verify(
            s =>
                s.DeleteObjectAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Given_MissingId_When_Delete_Then_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/v1/photos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_PhotoInComparison_When_Delete_Then_Returns409()
    {
        SetupS3Delete();
        var referencedId = await SeedPhotoInComparison();

        var response = await Client.DeleteAsync($"/api/v1/photos/{referencedId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();
        (await db.Photos.AnyAsync(p => p.Id == referencedId)).Should().BeTrue();
    }

    private void SetupS3Delete() =>
        Factory
            .S3Mock.Setup(s =>
                s.DeleteObjectAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new DeleteObjectResponse());

    private async Task<Guid> SeedPhoto()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();
        var photo = Photo.Create(
            $"photos/{Guid.NewGuid()}.jpg",
            "image/jpeg",
            DateTimeOffset.UtcNow
        );
        db.Photos.Add(photo);
        await db.SaveChangesAsync();
        return photo.Id;
    }

    private async Task<Guid> SeedPhotoInComparison()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();

        var before = Photo.Create(
            $"photos/{Guid.NewGuid()}.jpg",
            "image/jpeg",
            DateTimeOffset.UtcNow
        );
        var after = Photo.Create(
            $"photos/{Guid.NewGuid()}.jpg",
            "image/jpeg",
            DateTimeOffset.UtcNow
        );
        db.Photos.AddRange(before, after);

        db.Comparisons.Add(
            Comparison.Create(
                before.Id,
                after.Id,
                "Mild improvement.",
                [new ComparisonObservation("centre", "redness reduced", "less pronounced")],
                SeverityTrend.Improved,
                DateTimeOffset.UtcNow
            )
        );
        await db.SaveChangesAsync();
        return before.Id;
    }
}

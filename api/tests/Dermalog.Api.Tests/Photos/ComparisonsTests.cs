using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class ComparisonsTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    private const string PresignedUrl = "https://example.s3.amazonaws.com/x?sig=z";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Given_NoComparisons_When_GetLatest_Then_Returns204()
    {
        var response = await Client.GetAsync("/api/v1/photos/comparisons/latest");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Given_MultipleComparisons_When_GetLatest_Then_ReturnsNewest()
    {
        SetupPresign();
        await SeedComparison(SeverityTrend.Worsened, DateTimeOffset.UtcNow.AddDays(-3));
        var newer = await SeedComparison(SeverityTrend.Improved, DateTimeOffset.UtcNow.AddDays(-1));

        var response = await Client.GetAsync("/api/v1/photos/comparisons/latest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ComparisonDto>(JsonOptions);
        body!.Id.Should().Be(newer);
        body.SeverityTrend.Should().Be(SeverityTrend.Improved);
        body.Before.Url.Should().Be(PresignedUrl);
        body.After.Url.Should().Be(PresignedUrl);
    }

    [Fact]
    public async Task Given_StoredComparison_When_GetLatest_Then_SeverityTrendIsStringName()
    {
        SetupPresign();
        await SeedComparison(SeverityTrend.Similar, DateTimeOffset.UtcNow);

        var response = await Client.GetAsync("/api/v1/photos/comparisons/latest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var trend = JsonDocument.Parse(json).RootElement.GetProperty("severityTrend");
        trend.ValueKind.Should().Be(JsonValueKind.String);
        trend.GetString().Should().Be("Similar");
    }

    private void SetupPresign()
    {
        Factory
            .S3Mock.Setup(s =>
                s.GetPreSignedURLAsync(It.IsAny<Amazon.S3.Model.GetPreSignedUrlRequest>())
            )
            .ReturnsAsync(PresignedUrl);
    }

    private async Task<Guid> SeedComparison(SeverityTrend trend, DateTimeOffset generatedAt)
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

        var comparison = Comparison.Create(
            before.Id,
            after.Id,
            "Mild improvement in the central patch.",
            [new ComparisonObservation("centre", "redness reduced", "less pronounced")],
            trend,
            generatedAt
        );
        db.Comparisons.Add(comparison);
        await db.SaveChangesAsync();
        return comparison.Id;
    }
}

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

    [Fact]
    public async Task Given_MultipleComparisons_When_List_Then_ReturnsNewestFirst()
    {
        SetupPresign();
        var older = await SeedComparison(SeverityTrend.Worsened, DateTimeOffset.UtcNow.AddDays(-5));
        var newer = await SeedComparison(SeverityTrend.Improved, DateTimeOffset.UtcNow.AddDays(-1));

        var response = await Client.GetAsync("/api/v1/photos/comparisons");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ComparisonDto>>(JsonOptions);
        body!.Should().HaveCount(2);
        body[0].Id.Should().Be(newer);
        body[1].Id.Should().Be(older);
    }

    [Fact]
    public async Task Given_Comparison_When_PatchLabel_Then_LabelIsPersisted()
    {
        SetupPresign();
        var id = await SeedComparison(SeverityTrend.Similar, DateTimeOffset.UtcNow);

        var response = await Client.PatchAsJsonAsync(
            $"/api/v1/photos/comparisons/{id}",
            new { label = "Left elbow" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ComparisonDto>(JsonOptions);
        body!.Label.Should().Be("Left elbow");
        body.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task Given_Comparison_When_PatchComplete_Then_IsCompleteIsTrue()
    {
        SetupPresign();
        var id = await SeedComparison(SeverityTrend.Similar, DateTimeOffset.UtcNow);

        var response = await Client.PatchAsJsonAsync(
            $"/api/v1/photos/comparisons/{id}",
            new { isComplete = true }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ComparisonDto>(JsonOptions);
        body!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Given_MissingId_When_Patch_Then_Returns404()
    {
        var response = await Client.PatchAsJsonAsync(
            $"/api/v1/photos/comparisons/{Guid.NewGuid()}",
            new { label = "x" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_Comparison_When_Delete_Then_RowIsRemoved()
    {
        SetupPresign();
        var id = await SeedComparison(SeverityTrend.Similar, DateTimeOffset.UtcNow);

        var response = await Client.DeleteAsync($"/api/v1/photos/comparisons/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await Client.GetAsync("/api/v1/photos/comparisons");
        var body = await list.Content.ReadFromJsonAsync<List<ComparisonDto>>(JsonOptions);
        body!.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MissingId_When_Delete_Then_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/v1/photos/comparisons/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

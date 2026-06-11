using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class InsightTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Given_FewerThan3Comparisons_When_Get_Then_Returns204()
    {
        await SeedComparisons(2);

        var response = await Client.GetAsync("/api/v1/insight");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        VerifyBedrock(Times.Never());
    }

    [Fact]
    public async Task Given_3Comparisons_When_Get_Then_GeneratesAndReturns()
    {
        SetupBedrockInsight();
        await SeedComparisons(3);

        var response = await Client.GetAsync("/api/v1/insight");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<InsightDto>(JsonOptions);
        body!.Headline.Should().NotBeNullOrEmpty();
        body.Body.Should().NotBeNullOrEmpty();
        body.BasisComparisonCount.Should().Be(3);
    }

    [Fact]
    public async Task Given_UnchangedData_When_GetTwice_Then_BedrockCalledOnce()
    {
        SetupBedrockInsight();
        await SeedComparisons(3);

        await Client.GetAsync("/api/v1/insight");
        await Client.GetAsync("/api/v1/insight");

        VerifyBedrock(Times.Once());
    }

    [Fact]
    public async Task Given_NewComparison_When_Get_Then_Regenerates()
    {
        SetupBedrockInsight();
        await SeedComparisons(3);
        await Client.GetAsync("/api/v1/insight");

        await SeedComparisons(1);
        await Client.GetAsync("/api/v1/insight");

        VerifyBedrock(Times.Exactly(2));
    }

    private void SetupBedrockInsight()
    {
        var json = """
            {
              "headline": "Trending the right way",
              "body": "Across your recent comparisons, most show improvement. Lighting varied between shots, so read loosely."
            }
            """;
        var element = JsonDocument.Parse(json).RootElement.Clone();

        Factory
            .BedrockMock.Setup(b =>
                b.InvokeWithToolAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<BedrockImage>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(element);
    }

    private void VerifyBedrock(Times times) =>
        Factory.BedrockMock.Verify(
            b =>
                b.InvokeWithToolAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<BedrockImage>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            times
        );

    private async Task SeedComparisons(int count)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermalogDbContext>();
        var existing = await db.Comparisons.CountAsync();

        for (var i = 0; i < count; i++)
        {
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
                    "Mild improvement in the central patch.",
                    [new ComparisonObservation("centre", "redness reduced", "less pronounced")],
                    SeverityTrend.Improved,
                    DateTimeOffset.UtcNow.AddMinutes(existing + i)
                )
            );
        }

        await db.SaveChangesAsync();
    }
}

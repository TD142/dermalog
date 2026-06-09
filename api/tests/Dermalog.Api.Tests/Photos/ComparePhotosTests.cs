using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class ComparePhotosTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    private const string PresignedUrl = "https://example.s3.amazonaws.com/x?sig=z";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Given_ValidIds_When_Compare_Then_Returns200WithStructuredResult()
    {
        SetupPresignAndS3();
        SetupBedrockSuccess();

        var beforeId = await CreatePhoto("photos/before.jpg");
        var afterId = await CreatePhoto("photos/after.jpg");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(beforeId, afterId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ComparisonResult>(JsonOptions);
        body.Should().NotBeNull();
        body!.OverallSummary.Should().NotBeNullOrEmpty();
        body.Observations.Should().NotBeEmpty();
        body.SeverityTrend.Should().Be(SeverityTrend.Improved);
        body.GeneratedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Given_ValidIds_When_Compare_Then_SeverityTrendSerializesAsStringName()
    {
        SetupPresignAndS3();
        SetupBedrockSuccess();

        var beforeId = await CreatePhoto("photos/before.jpg");
        var afterId = await CreatePhoto("photos/after.jpg");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(beforeId, afterId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var trend = JsonDocument.Parse(json).RootElement.GetProperty("severityTrend");
        trend.ValueKind.Should().Be(JsonValueKind.String);
        trend.GetString().Should().Be("Improved");
    }

    [Fact]
    public async Task Given_MissingPhoto_When_Compare_Then_Returns404()
    {
        SetupPresignAndS3();
        var beforeId = await CreatePhoto("photos/exists.jpg");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(beforeId, Guid.NewGuid())
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
            Times.Never
        );
    }

    [Fact]
    public async Task Given_SameIdTwice_When_Compare_Then_Returns400()
    {
        var id = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(id, id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_BedrockFails_When_Compare_Then_Returns502()
    {
        SetupPresignAndS3();

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
            .ThrowsAsync(new AmazonBedrockRuntimeException("model unavailable"));

        var beforeId = await CreatePhoto("photos/b.jpg");
        var afterId = await CreatePhoto("photos/a.jpg");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(beforeId, afterId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Given_ValidIds_When_Compare_Then_BedrockReceivesBothImageBytes()
    {
        SetupPresignAndS3();
        SetupBedrockSuccess();

        // Override the S3 setup with content we can identify
        var beforeBytes = new byte[] { 1, 2, 3, 4 };
        var afterBytes = new byte[] { 9, 9, 9, 9, 9 };
        Factory.SetupS3Object("photos/captured-bytes-before.jpg", beforeBytes);
        Factory.SetupS3Object("photos/captured-bytes-after.jpg", afterBytes);

        var beforeId = await CreatePhoto("photos/captured-bytes-before.jpg");
        var afterId = await CreatePhoto("photos/captured-bytes-after.jpg");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/compare",
            new ComparePhotosRequest(beforeId, afterId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Factory.BedrockMock.Verify(
            b =>
                b.InvokeWithToolAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<IReadOnlyList<BedrockImage>>(images =>
                        images.Count == 2
                        && images[0].Bytes.SequenceEqual(beforeBytes)
                        && images[1].Bytes.SequenceEqual(afterBytes)
                    ),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private void SetupPresignAndS3()
    {
        // Used by ConfirmAsync to embed a pre-signed URL in PhotoDto.
        Factory
            .S3Mock.Setup(s =>
                s.GetPreSignedURLAsync(It.IsAny<Amazon.S3.Model.GetPreSignedUrlRequest>())
            )
            .ReturnsAsync(PresignedUrl);

        // Default fake content for any object the comparison flow downloads.
        Factory.SetupS3Object("photos/before.jpg", new byte[] { 1, 2, 3 });
        Factory.SetupS3Object("photos/after.jpg", new byte[] { 4, 5, 6 });
        Factory.SetupS3Object("photos/b.jpg", new byte[] { 1 });
        Factory.SetupS3Object("photos/a.jpg", new byte[] { 2 });
        Factory.SetupS3Object("photos/exists.jpg", new byte[] { 0 });
    }

    private void SetupBedrockSuccess()
    {
        var toolInputJson = """
            {
              "overallSummary": "Mild improvement in the central patch.",
              "observations": [
                { "area": "centre", "change": "redness reduced", "notes": "less pronounced than before" }
              ],
              "severityTrend": "improved"
            }
            """;

        var element = JsonDocument.Parse(toolInputJson).RootElement.Clone();

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

    private async Task<Guid> CreatePhoto(string objectKey)
    {
        var request = new ConfirmPhotoRequest(objectKey, "image/jpeg", DateTimeOffset.UtcNow);
        var response = await Client.PostAsJsonAsync("/api/v1/photos", request);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PhotoDto>();
        return dto!.Id;
    }
}

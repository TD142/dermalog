using System.Net;
using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class CreateUploadUrlTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Given_ValidContentType_When_RequestUploadUrl_Then_Returns200WithUrl()
    {
        const string fakeUrl = "https://example.s3.amazonaws.com/photos/foo.jpg?sig=abc";
        Factory
            .S3Mock.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(fakeUrl);

        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/upload-url",
            new UploadUrlRequest("image/jpeg")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<UploadUrlResponse>();
        body.Should().NotBeNull();
        body!.UploadUrl.Should().Be(fakeUrl);
        body.ObjectKey.Should().StartWith("photos/").And.EndWith(".jpg");
        body.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Given_EmptyContentType_When_RequestUploadUrl_Then_Returns400()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/photos/upload-url",
            new UploadUrlRequest("")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Factory.S3Mock.Verify(
            s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()),
            Times.Never
        );
    }
}

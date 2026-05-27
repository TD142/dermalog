using System.Net;
using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Dermalog.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class CreateUploadUrlTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAmazonS3> _s3 = new();

    public CreateUploadUrlTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAmazonS3>();
                services.AddSingleton(_s3.Object);
            });
        });
    }

    [Fact]
    public async Task Given_ValidContentType_When_RequestUploadUrl_Then_Returns200WithUrl()
    {
        const string fakeUrl = "https://example.s3.amazonaws.com/photos/foo.jpg?sig=abc";
        _s3.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(fakeUrl);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
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
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/photos/upload-url",
            new UploadUrlRequest("")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _s3.Verify(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()), Times.Never);
    }
}

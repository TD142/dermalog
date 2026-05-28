using System.Net;
using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Moq;

namespace Dermalog.Api.Tests.Photos;

public class ListPhotosTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Given_NoPhotos_When_List_Then_ReturnsEmpty()
    {
        var response = await Client.GetAsync("/api/v1/photos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PhotoDto>>();
        body.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Given_ThreePhotos_When_List_Then_ReturnsNewestFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await Confirm("photos/a.jpg", now.AddHours(-2));
        await Confirm("photos/b.jpg", now);
        await Confirm("photos/c.jpg", now.AddHours(-1));

        var response = await Client.GetAsync("/api/v1/photos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PhotoDto>>();
        body.Should().NotBeNull();
        body!
            .Select(p => p.ObjectKey)
            .Should()
            .Equal("photos/b.jpg", "photos/c.jpg", "photos/a.jpg");
    }

    [Fact]
    public async Task Given_PhotosExceedingTake_When_List_Then_RespectsTakeLimit()
    {
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 5; i++)
        {
            await Confirm($"photos/n-{i}.jpg", now.AddMinutes(-i));
        }

        var response = await Client.GetAsync("/api/v1/photos?take=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PhotoDto>>();
        body.Should().NotBeNull();
        body!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_PhotoExists_When_List_Then_ResponseIncludesPresignedUrl()
    {
        const string fakeUrl = "https://example.s3.amazonaws.com/photos/list.jpg?sig=xyz";
        Factory
            .S3Mock.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(fakeUrl);

        await Confirm("photos/url-test.jpg", DateTimeOffset.UtcNow);

        var response = await Client.GetAsync("/api/v1/photos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<PhotoDto>>();
        body.Should().NotBeNull().And.HaveCount(1);
        body![0].Url.Should().Be(fakeUrl);
        body[0].UrlExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Given_PhotoExists_When_List_Then_S3MockReceivedGetPresignRequest()
    {
        const string fakeUrl = "https://example.s3.amazonaws.com/photos/verify.jpg?sig=xyz";
        Factory
            .S3Mock.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(fakeUrl);

        await Confirm("photos/verify-key.jpg", DateTimeOffset.UtcNow);

        await Client.GetAsync("/api/v1/photos");

        Factory.S3Mock.Verify(
            s =>
                s.GetPreSignedURLAsync(
                    It.Is<GetPreSignedUrlRequest>(r =>
                        r.Verb == HttpVerb.GET && r.Key == "photos/verify-key.jpg"
                    )
                ),
            Times.AtLeastOnce
        );
    }

    private async Task Confirm(string objectKey, DateTimeOffset capturedAt)
    {
        var request = new ConfirmPhotoRequest(objectKey, "image/jpeg", capturedAt);
        var response = await Client.PostAsJsonAsync("/api/v1/photos", request);
        response.EnsureSuccessStatusCode();
    }
}

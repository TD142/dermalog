using System.Net;
using System.Net.Http.Json;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;

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

    private async Task Confirm(string objectKey, DateTimeOffset capturedAt)
    {
        var request = new ConfirmPhotoRequest(objectKey, "image/jpeg", capturedAt);
        var response = await Client.PostAsJsonAsync("/api/v1/photos", request);
        response.EnsureSuccessStatusCode();
    }
}

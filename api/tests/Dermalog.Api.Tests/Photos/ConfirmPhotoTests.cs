using System.Net;
using System.Net.Http.Json;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;

namespace Dermalog.Api.Tests.Photos;

public class ConfirmPhotoTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Given_ValidRequest_When_Confirm_Then_Returns201WithDto()
    {
        var request = new ConfirmPhotoRequest(
            ObjectKey: "photos/2026/05/27/abc123.jpg",
            ContentType: "image/jpeg",
            CapturedAt: DateTimeOffset.UtcNow
        );

        var response = await Client.PostAsJsonAsync("/api/v1/photos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<PhotoDto>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.ObjectKey.Should().Be(request.ObjectKey);
        body.ContentType.Should().Be(request.ContentType);
    }

    [Fact]
    public async Task Given_DuplicateObjectKey_When_Confirm_Then_Returns409()
    {
        var request = new ConfirmPhotoRequest(
            ObjectKey: "photos/2026/05/27/duplicate.jpg",
            ContentType: "image/jpeg",
            CapturedAt: DateTimeOffset.UtcNow
        );

        var first = await Client.PostAsJsonAsync("/api/v1/photos", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await Client.PostAsJsonAsync("/api/v1/photos", request);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Given_EmptyObjectKey_When_Confirm_Then_Returns400()
    {
        var request = new ConfirmPhotoRequest(
            ObjectKey: "",
            ContentType: "image/jpeg",
            CapturedAt: DateTimeOffset.UtcNow
        );

        var response = await Client.PostAsJsonAsync("/api/v1/photos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

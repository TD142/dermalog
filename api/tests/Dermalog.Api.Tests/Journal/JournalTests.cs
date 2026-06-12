using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dermalog.Api.Domain;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Models;
using Dermalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Moq;

namespace Dermalog.Api.Tests.Journal;

public class JournalTests(DermalogAppFactory factory) : IntegrationTestBase(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Given_Text_When_Create_Then_ParsesAndPersistsTags()
    {
        SetupBedrockTags();

        var response = await Client.PostAsJsonAsync(
            "/api/v1/journal",
            new CreateJournalEntryRequest(
                "Elbows itchy, think it's the new detergent. Used hydrocortisone."
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JournalEntryDto>(JsonOptions);
        body!.Id.Should().NotBeEmpty();
        body.Symptoms.Should().Contain("itchiness");
        body.Triggers.Should().Contain("new detergent");
        body.Treatments.Should().Contain("hydrocortisone");
        body.Areas.Should().Contain("elbows");
        body.Severity.Should().Be(JournalSeverity.Moderate);
        body.Summary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Given_EmptyText_When_Create_Then_Returns400()
    {
        SetupBedrockTags();

        var response = await Client.PostAsJsonAsync(
            "/api/v1/journal",
            new CreateJournalEntryRequest("   ")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
    public async Task Given_MultipleEntries_When_List_Then_ReturnsNewestFirst()
    {
        SetupBedrockTags();
        await Client.PostAsJsonAsync("/api/v1/journal", new CreateJournalEntryRequest("first"));
        await Task.Delay(10);
        await Client.PostAsJsonAsync("/api/v1/journal", new CreateJournalEntryRequest("second"));

        var response = await Client.GetAsync("/api/v1/journal");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<JournalEntryDto>>(JsonOptions);
        body!.Should().HaveCount(2);
        body[0].Text.Should().Be("second");
        body[1].Text.Should().Be("first");
    }

    [Fact]
    public async Task Given_Entry_When_Delete_Then_RemovesIt()
    {
        SetupBedrockTags();
        var created = await Client.PostAsJsonAsync(
            "/api/v1/journal",
            new CreateJournalEntryRequest("entry to delete")
        );
        var entry = await created.Content.ReadFromJsonAsync<JournalEntryDto>(JsonOptions);

        var response = await Client.DeleteAsync($"/api/v1/journal/{entry!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await Client.GetAsync("/api/v1/journal");
        var body = await list.Content.ReadFromJsonAsync<List<JournalEntryDto>>(JsonOptions);
        body!.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MissingId_When_Delete_Then_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/v1/journal/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void SetupBedrockTags()
    {
        var json = """
            {
              "symptoms": ["itchiness"],
              "triggers": ["new detergent"],
              "treatments": ["hydrocortisone"],
              "areas": ["elbows"],
              "severity": "moderate",
              "summary": "Itchy elbows, suspected new detergent, used hydrocortisone."
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
}

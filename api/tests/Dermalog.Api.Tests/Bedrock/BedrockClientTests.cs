using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Dermalog.Api.Infrastructure.Bedrock;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Dermalog.Api.Tests.Bedrock;

public class BedrockClientTests
{
    private const string ToolName = "submit_comparison";

    [Fact]
    public async Task InvokeWithToolAsync_SendsLowTemperatureInRequestBody()
    {
        InvokeModelRequest? captured = null;

        var bedrock = new Mock<IAmazonBedrockRuntime>();
        bedrock
            .Setup(b =>
                b.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>())
            )
            .Callback<InvokeModelRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(() =>
                new InvokeModelResponse
                {
                    Body = new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            $$"""
                            {
                              "content": [
                                { "type": "tool_use", "name": "{{ToolName}}", "input": { "ok": true } }
                              ]
                            }
                            """
                        )
                    ),
                }
            );

        var client = new BedrockClient(bedrock.Object, NullLogger<BedrockClient>.Instance);

        await client.InvokeWithToolAsync(
            modelId: "anthropic.claude-sonnet-4-6",
            systemPrompt: "system",
            userPrompt: "user",
            images: [],
            toolName: ToolName,
            toolDescription: "desc",
            toolInputJsonSchema: """{ "type": "object", "properties": {} }""",
            ct: CancellationToken.None
        );

        captured.Should().NotBeNull();
        captured!.Body.Position = 0;
        var bodyJson = await new StreamReader(captured.Body).ReadToEndAsync();

        using var doc = JsonDocument.Parse(bodyJson);
        doc.RootElement.TryGetProperty("temperature", out var temperature).Should().BeTrue();
        temperature.GetDouble().Should().BeLessThanOrEqualTo(0.2);
    }
}

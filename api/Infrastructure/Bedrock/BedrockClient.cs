using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

namespace Dermalog.Api.Infrastructure.Bedrock;

public class BedrockClient(IAmazonBedrockRuntime bedrock, ILogger<BedrockClient> logger)
    : IBedrockClient
{
    private const int MaxTokens = 2048;
    private const double Temperature = 0d;

    public async Task<JsonElement> InvokeWithToolAsync(
        string modelId,
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<BedrockImage> images,
        string toolName,
        string toolDescription,
        string toolInputJsonSchema,
        CancellationToken ct
    )
    {
        var body = BuildBody(
            systemPrompt,
            userPrompt,
            images,
            toolName,
            toolDescription,
            toolInputJsonSchema
        );

        var request = new InvokeModelRequest
        {
            ModelId = modelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(body)),
        };

        var response = await bedrock.InvokeModelAsync(request, ct);

        using var reader = new StreamReader(response.Body);
        var responseJson = await reader.ReadToEndAsync(ct);

        logger.LogInformation("Bedrock model {ModelId} invocation completed", modelId);

        return ExtractToolInput(responseJson, toolName);
    }

    private static string BuildBody(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<BedrockImage> images,
        string toolName,
        string toolDescription,
        string toolInputJsonSchema
    )
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("anthropic_version", "bedrock-2023-05-31");
            writer.WriteNumber("max_tokens", MaxTokens);
            writer.WriteNumber("temperature", Temperature);
            writer.WriteStartArray("system");
            writer.WriteStartObject();
            writer.WriteString("type", "text");
            writer.WriteString("text", systemPrompt);
            writer.WriteStartObject("cache_control");
            writer.WriteString("type", "ephemeral");
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteStartArray("tools");
            writer.WriteStartObject();
            writer.WriteString("name", toolName);
            writer.WriteString("description", toolDescription);
            writer.WritePropertyName("input_schema");
            using (var schemaDoc = JsonDocument.Parse(toolInputJsonSchema))
            {
                schemaDoc.RootElement.WriteTo(writer);
            }
            writer.WriteStartObject("cache_control");
            writer.WriteString("type", "ephemeral");
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteStartObject("tool_choice");
            writer.WriteString("type", "tool");
            writer.WriteString("name", toolName);
            writer.WriteEndObject();
            writer.WriteStartArray("messages");
            writer.WriteStartObject();
            writer.WriteString("role", "user");
            writer.WriteStartArray("content");

            foreach (var image in images)
            {
                writer.WriteStartObject();
                writer.WriteString("type", "image");
                writer.WriteStartObject("source");
                writer.WriteString("type", "base64");
                writer.WriteString("media_type", image.MediaType);
                writer.WriteString("data", Convert.ToBase64String(image.Bytes));
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteStartObject();
            writer.WriteString("type", "text");
            writer.WriteString("text", userPrompt);
            writer.WriteEndObject();

            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static JsonElement ExtractToolInput(string responseJson, string toolName)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var content = doc.RootElement.GetProperty("content");

        foreach (var block in content.EnumerateArray())
        {
            if (
                block.TryGetProperty("type", out var type)
                && type.GetString() == "tool_use"
                && block.TryGetProperty("name", out var name)
                && name.GetString() == toolName
            )
            {
                return block.GetProperty("input").Clone();
            }
        }

        throw new InvalidOperationException(
            $"Bedrock response did not contain a tool_use block for '{toolName}'."
        );
    }
}

using System.Text.Json;

namespace Dermalog.Api.Infrastructure.Bedrock;

public interface IBedrockClient
{
    /// <summary>
    /// Invokes a Claude model on Bedrock with a forced tool call, returning the tool input as a JsonElement.
    /// The model is required to call <paramref name="toolName"/> with arguments matching <paramref name="toolInputJsonSchema"/>.
    /// System prompt + tool definition use ephemeral prompt caching.
    /// </summary>
    Task<JsonElement> InvokeWithToolAsync(
        string modelId,
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<BedrockImage> images,
        string toolName,
        string toolDescription,
        string toolInputJsonSchema,
        CancellationToken ct
    );
}

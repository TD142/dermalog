using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.S3;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Models;
using Dermalog.Api.Services;
using Microsoft.Extensions.Options;

namespace Dermalog.Api.Services.AI;

public class PhotoComparisonService(
    DermalogDbContext db,
    IAmazonS3 s3,
    IBedrockClient bedrock,
    IOptions<BedrockOptions> bedrockOptions,
    IOptions<PhotosOptions> photoOptions,
    ILogger<PhotoComparisonService> logger
) : IPhotoComparisonService
{
    private const string ToolName = "submit_comparison";

    private const string SystemPrompt =
        "You are an observational reporter comparing two photographs of the same area of skin, "
        + "taken at different times. Describe visible differences using observational, "
        + "non-diagnostic language ('appears reduced', 'similar to', 'no notable change', "
        + "'redness less pronounced'). NEVER make medical claims, diagnoses, or treatment "
        + "recommendations. Focus on observable visual changes: redness, dryness, area size, "
        + "visible texture or scaling. If a feature is hard to assess from the images "
        + "(lighting, angle, partial occlusion), say so honestly rather than guessing.";

    private const string ToolDescription =
        "Submit your structured observational comparison of the two photographs.";

    private const string ToolInputSchema = """
        {
          "type": "object",
          "properties": {
            "overallSummary": {
              "type": "string",
              "description": "1-2 sentence summary of the overall change between the two photos, in observational language."
            },
            "observations": {
              "type": "array",
              "description": "Per-area observations of visual differences.",
              "items": {
                "type": "object",
                "properties": {
                  "area": { "type": "string", "description": "Region of the photo (e.g. 'left elbow', 'centre patch')." },
                  "change": { "type": "string", "description": "Short description of the visual change observed." },
                  "notes": { "type": "string", "description": "Additional observational detail, including any uncertainty due to lighting or angle." }
                },
                "required": ["area", "change", "notes"]
              }
            },
            "severityTrend": {
              "type": "string",
              "enum": ["improved", "similar", "worsened"],
              "description": "Overall trend based on visible changes."
            }
          },
          "required": ["overallSummary", "observations", "severityTrend"]
        }
        """;

    public async Task<ServiceResult<ComparisonResult>> CompareAsync(
        ComparePhotosRequest request,
        CancellationToken ct
    )
    {
        var before = await db.Photos.FindAsync([request.BeforeId], ct);
        var after = await db.Photos.FindAsync([request.AfterId], ct);

        if (before is null || after is null)
        {
            return ServiceResult<ComparisonResult>.Failure(
                ServiceResultError.NotFound,
                "One or both photos were not found"
            );
        }

        BedrockImage beforeImage;
        BedrockImage afterImage;
        try
        {
            beforeImage = await DownloadAsync(before, ct);
            afterImage = await DownloadAsync(after, ct);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "Failed to download photo bytes from S3 for comparison");
            return ServiceResult<ComparisonResult>.Failure(
                ServiceResultError.External,
                "Failed to fetch photo bytes from storage"
            );
        }

        var userPrompt =
            $"Compare these two photos. The first was captured on "
            + $"{before.CapturedAt:yyyy-MM-dd}, the second on {after.CapturedAt:yyyy-MM-dd}. "
            + $"Call submit_comparison with your observations.";

        try
        {
            var input = await bedrock.InvokeWithToolAsync(
                modelId: bedrockOptions.Value.ComparisonModelId,
                systemPrompt: SystemPrompt,
                userPrompt: userPrompt,
                images: [beforeImage, afterImage],
                toolName: ToolName,
                toolDescription: ToolDescription,
                toolInputJsonSchema: ToolInputSchema,
                ct: ct
            );

            return ServiceResult<ComparisonResult>.Success(Map(input));
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            logger.LogError(ex, "Bedrock invocation failed");
            return ServiceResult<ComparisonResult>.Failure(
                ServiceResultError.External,
                "Comparison model invocation failed"
            );
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Bedrock returned an unexpected response shape");
            return ServiceResult<ComparisonResult>.Failure(
                ServiceResultError.External,
                "Comparison model returned an unexpected response"
            );
        }
    }

    private async Task<BedrockImage> DownloadAsync(Photo photo, CancellationToken ct)
    {
        using var response = await s3.GetObjectAsync(
            photoOptions.Value.BucketName,
            photo.ObjectKey,
            ct
        );
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return new BedrockImage(photo.ContentType, ms.ToArray());
    }

    private static ComparisonResult Map(JsonElement input)
    {
        var summary = input.GetProperty("overallSummary").GetString() ?? "";
        var trendStr = input.GetProperty("severityTrend").GetString() ?? "similar";
        var trend = Enum.Parse<SeverityTrend>(trendStr, ignoreCase: true);

        var observations = input
            .GetProperty("observations")
            .EnumerateArray()
            .Select(o => new Observation(
                o.GetProperty("area").GetString() ?? "",
                o.GetProperty("change").GetString() ?? "",
                o.GetProperty("notes").GetString() ?? ""
            ))
            .ToList();

        return new ComparisonResult(summary, observations, trend, DateTimeOffset.UtcNow);
    }
}

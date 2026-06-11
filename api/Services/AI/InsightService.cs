using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dermalog.Api.Services.AI;

public class InsightService(
    DermalogDbContext db,
    IBedrockClient bedrock,
    IOptions<BedrockOptions> bedrockOptions,
    ILogger<InsightService> logger
) : IInsightService
{
    private const int MinComparisons = 3;
    private const string ToolName = "submit_insight";

    private const string SystemPrompt =
        "You are summarising one person's own skin-tracking history into a short, encouraging, "
        + "plain-language read of their progress. You are given pre-computed facts about their photo "
        + "comparisons (how many, over what period, and the trend of each). Use ONLY those facts. "
        + "Write observationally — describe what the logs show; never diagnose, never give medical or "
        + "treatment advice, never speculate about causes. If the trend is mixed or the data is thin, "
        + "say so honestly. Warm but factual.";

    private const string ToolDescription =
        "Submit a short progress insight for the user based only on the provided facts.";

    private const string ToolInputSchema = """
        {
          "type": "object",
          "properties": {
            "headline": { "type": "string", "description": "A short headline (max ~8 words) for the progress read." },
            "body": { "type": "string", "description": "2-3 sentences, observational, describing what the comparison history shows. No medical advice." }
          },
          "required": ["headline", "body"]
        }
        """;

    public async Task<ServiceResult<InsightDto?>> GetAsync(CancellationToken ct)
    {
        var comparisons = await db
            .Comparisons.AsNoTracking()
            .OrderByDescending(c => c.GeneratedAt)
            .ToListAsync(ct);

        if (comparisons.Count < MinComparisons)
        {
            return ServiceResult<InsightDto?>.Success(null);
        }

        var basisCount = comparisons.Count;
        var basisLatest = comparisons[0].GeneratedAt;

        var existing = await db
            .Insights.AsNoTracking()
            .OrderByDescending(i => i.GeneratedAt)
            .FirstOrDefaultAsync(ct);

        if (
            existing is not null
            && existing.BasisComparisonCount == basisCount
            && existing.BasisLatestComparisonAt == basisLatest
        )
        {
            return ServiceResult<InsightDto?>.Success(existing.ToDto());
        }

        JsonElement input;
        try
        {
            input = await bedrock.InvokeWithToolAsync(
                modelId: bedrockOptions.Value.ComparisonModelId,
                systemPrompt: SystemPrompt,
                userPrompt: BuildDigest(comparisons),
                images: [],
                toolName: ToolName,
                toolDescription: ToolDescription,
                toolInputJsonSchema: ToolInputSchema,
                ct: ct
            );
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            logger.LogError(ex, "Bedrock invocation failed generating insight");
            return ServiceResult<InsightDto?>.Failure(
                ServiceResultError.External,
                "Insight generation failed"
            );
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Bedrock returned an unexpected response generating insight");
            return ServiceResult<InsightDto?>.Failure(
                ServiceResultError.External,
                "Insight generation returned an unexpected response"
            );
        }

        var headline = input.GetProperty("headline").GetString() ?? "Your progress";
        var body = input.GetProperty("body").GetString() ?? "";
        db.Insights.RemoveRange(await db.Insights.ToListAsync(ct));
        var insight = Insight.Create(headline, body, basisCount, basisLatest);
        db.Insights.Add(insight);
        await db.SaveChangesAsync(ct);

        return ServiceResult<InsightDto?>.Success(insight.ToDto());
    }

    private static string BuildDigest(IReadOnlyList<Comparison> comparisons)
    {
        var improved = comparisons.Count(c => c.SeverityTrend == SeverityTrend.Improved);
        var similar = comparisons.Count(c => c.SeverityTrend == SeverityTrend.Similar);
        var worsened = comparisons.Count(c => c.SeverityTrend == SeverityTrend.Worsened);
        var earliest = comparisons.Min(c => c.GeneratedAt);
        var latest = comparisons.Max(c => c.GeneratedAt);
        var labels = comparisons
            .Select(c => c.Label)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Distinct()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Facts about the user's photo comparisons (use only these):");
        sb.AppendLine($"- Total comparisons: {comparisons.Count}");
        sb.AppendLine($"- Date range: {earliest:yyyy-MM-dd} to {latest:yyyy-MM-dd}");
        sb.AppendLine(
            $"- Trend counts: improved={improved}, similar={similar}, worsened={worsened}"
        );
        if (labels.Count > 0)
        {
            sb.AppendLine($"- Areas/labels tracked: {string.Join(", ", labels)}");
        }
        sb.AppendLine(
            "Call submit_insight with a short headline and a 2-3 sentence observational read."
        );
        return sb.ToString();
    }
}

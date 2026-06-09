namespace Dermalog.Api.Models;

public enum SeverityTrend
{
    Improved,
    Similar,
    Worsened,
}

public record Observation(string Area, string Change, string Notes);

public record ComparisonResult(
    string OverallSummary,
    IReadOnlyList<Observation> Observations,
    SeverityTrend SeverityTrend,
    DateTimeOffset GeneratedAt
);

using Dermalog.Api.Domain;

namespace Dermalog.Api.Models;

public record Observation(string Area, string Change, string Notes);

public record ComparisonDto(
    Guid Id,
    PhotoDto Before,
    PhotoDto After,
    string OverallSummary,
    IReadOnlyList<Observation> Observations,
    SeverityTrend SeverityTrend,
    DateTimeOffset GeneratedAt,
    string? Label,
    bool IsComplete
);

public record UpdateComparisonRequest(string? Label, bool? IsComplete);

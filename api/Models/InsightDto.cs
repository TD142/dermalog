namespace Dermalog.Api.Models;

public record InsightDto(
    string Headline,
    string Body,
    DateTimeOffset GeneratedAt,
    int BasisComparisonCount
);

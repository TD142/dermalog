using Dermalog.Api.Domain;

namespace Dermalog.Api.Models;

public static class InsightMappings
{
    public static InsightDto ToDto(this Insight insight) =>
        new(insight.Headline, insight.Body, insight.GeneratedAt, insight.BasisComparisonCount);
}

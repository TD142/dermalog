namespace Dermalog.Api.Domain;

public class Comparison
{
    public Guid Id { get; private set; }
    public Guid BeforePhotoId { get; private set; }
    public Guid AfterPhotoId { get; private set; }
    public string OverallSummary { get; private set; } = default!;
    public IReadOnlyList<ComparisonObservation> Observations { get; private set; } = [];
    public SeverityTrend SeverityTrend { get; private set; }
    public string? Label { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Comparison() { }

    public void SetLabel(string? label) =>
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();

    public void SetComplete(bool complete) => CompletedAt = complete ? DateTimeOffset.UtcNow : null;

    public static Comparison Create(
        Guid beforePhotoId,
        Guid afterPhotoId,
        string overallSummary,
        IReadOnlyList<ComparisonObservation> observations,
        SeverityTrend severityTrend,
        DateTimeOffset generatedAt
    )
    {
        return new Comparison
        {
            Id = Guid.NewGuid(),
            BeforePhotoId = beforePhotoId,
            AfterPhotoId = afterPhotoId,
            OverallSummary = overallSummary,
            Observations = observations,
            SeverityTrend = severityTrend,
            GeneratedAt = generatedAt,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}

public record ComparisonObservation(string Area, string Change, string Notes);

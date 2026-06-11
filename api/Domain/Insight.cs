namespace Dermalog.Api.Domain;

public class Insight
{
    public Guid Id { get; private set; }
    public string Headline { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public int BasisComparisonCount { get; private set; }
    public DateTimeOffset BasisLatestComparisonAt { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }

    private Insight() { }

    public static Insight Create(
        string headline,
        string body,
        int basisComparisonCount,
        DateTimeOffset basisLatestComparisonAt
    )
    {
        return new Insight
        {
            Id = Guid.NewGuid(),
            Headline = headline,
            Body = body,
            BasisComparisonCount = basisComparisonCount,
            BasisLatestComparisonAt = basisLatestComparisonAt,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }
}

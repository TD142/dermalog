namespace Dermalog.Api.Domain;

public enum JournalSeverity
{
    Mild,
    Moderate,
    Severe,
}

public record JournalTags(
    IReadOnlyList<string> Symptoms,
    IReadOnlyList<string> Triggers,
    IReadOnlyList<string> Treatments,
    IReadOnlyList<string> Areas
);

public class JournalEntry
{
    public Guid Id { get; private set; }
    public string Text { get; private set; } = default!;
    public JournalTags Tags { get; private set; } = new([], [], [], []);
    public JournalSeverity Severity { get; private set; }
    public string Summary { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private JournalEntry() { }

    public static JournalEntry Create(
        string text,
        JournalTags tags,
        JournalSeverity severity,
        string summary
    )
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            Text = text,
            Tags = tags,
            Severity = severity,
            Summary = summary,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}

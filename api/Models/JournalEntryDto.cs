using Dermalog.Api.Domain;

namespace Dermalog.Api.Models;

public record JournalEntryDto(
    Guid Id,
    string Text,
    IReadOnlyList<string> Symptoms,
    IReadOnlyList<string> Triggers,
    IReadOnlyList<string> Treatments,
    IReadOnlyList<string> Areas,
    JournalSeverity Severity,
    string Summary,
    DateTimeOffset CreatedAt
);

public record CreateJournalEntryRequest(string Text);

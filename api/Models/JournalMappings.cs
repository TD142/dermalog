using Dermalog.Api.Domain;

namespace Dermalog.Api.Models;

public static class JournalMappings
{
    public static JournalEntryDto ToDto(this JournalEntry e) =>
        new(
            e.Id,
            e.Text,
            e.Tags.Symptoms,
            e.Tags.Triggers,
            e.Tags.Treatments,
            e.Tags.Areas,
            e.Severity,
            e.Summary,
            e.CreatedAt
        );
}

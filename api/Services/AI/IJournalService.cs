using Dermalog.Api.Models;

namespace Dermalog.Api.Services.AI;

public interface IJournalService
{
    Task<ServiceResult<JournalEntryDto>> CreateAsync(string text, CancellationToken ct);

    Task<ServiceResult<IReadOnlyList<JournalEntryDto>>> GetAllAsync(CancellationToken ct);

    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct);
}

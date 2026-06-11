using Dermalog.Api.Models;

namespace Dermalog.Api.Services.AI;

public interface IInsightService
{
    Task<ServiceResult<InsightDto?>> GetAsync(CancellationToken ct);
}

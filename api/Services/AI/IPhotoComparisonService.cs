using Dermalog.Api.Models;

namespace Dermalog.Api.Services.AI;

public interface IPhotoComparisonService
{
    Task<ServiceResult<ComparisonDto>> CompareAsync(
        ComparePhotosRequest request,
        CancellationToken ct
    );

    Task<ServiceResult<ComparisonDto?>> GetLatestAsync(CancellationToken ct);
}

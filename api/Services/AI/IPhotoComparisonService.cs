using Dermalog.Api.Models;

namespace Dermalog.Api.Services.AI;

public interface IPhotoComparisonService
{
    Task<ServiceResult<ComparisonDto>> CompareAsync(
        ComparePhotosRequest request,
        CancellationToken ct
    );

    Task<ServiceResult<ComparisonDto?>> GetLatestAsync(CancellationToken ct);

    Task<ServiceResult<IReadOnlyList<ComparisonDto>>> GetAllAsync(CancellationToken ct);

    Task<ServiceResult<ComparisonDto>> UpdateAsync(
        Guid id,
        string? label,
        bool? isComplete,
        CancellationToken ct
    );

    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct);
}

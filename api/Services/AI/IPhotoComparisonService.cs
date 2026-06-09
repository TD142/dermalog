using Dermalog.Api.Models;

namespace Dermalog.Api.Services.AI;

public interface IPhotoComparisonService
{
    Task<ServiceResult<ComparisonResult>> CompareAsync(
        ComparePhotosRequest request,
        CancellationToken ct
    );
}

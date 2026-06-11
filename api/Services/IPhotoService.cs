using Dermalog.Api.Models;

namespace Dermalog.Api.Services;

public interface IPhotoService
{
    Task<ServiceResult<PhotoDto>> ConfirmAsync(ConfirmPhotoRequest request, CancellationToken ct);

    Task<ServiceResult<IReadOnlyList<PhotoDto>>> ListAsync(int take, CancellationToken ct);

    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct);
}

using Dermalog.Api.Models;

namespace Dermalog.Api.Services;

public interface IPhotoUploadService
{
    Task<UploadUrlResponse> CreateUploadUrlAsync(UploadUrlRequest request, CancellationToken ct);
}

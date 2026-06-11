using Dermalog.Api.Models;

namespace Dermalog.Api.Services;

public interface IPhotoStorageService
{
    Task<ServiceResult<UploadUrlResponse>> CreateUploadUrlAsync(
        UploadUrlRequest request,
        CancellationToken ct
    );

    Task<string> CreateDownloadUrlAsync(
        string objectKey,
        DateTimeOffset expiresAt,
        CancellationToken ct
    );

    Task DeleteObjectAsync(string objectKey, CancellationToken ct);
}

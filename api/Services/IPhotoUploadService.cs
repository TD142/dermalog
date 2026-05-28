using Dermalog.Api.Models;

namespace Dermalog.Api.Services;

public interface IPhotoUploadService
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
}

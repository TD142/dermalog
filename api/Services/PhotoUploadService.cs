using Amazon.S3;
using Amazon.S3.Model;
using Dermalog.Api.Models;
using Microsoft.Extensions.Options;

namespace Dermalog.Api.Services;

public class PhotoUploadService(
    IAmazonS3 s3,
    IOptions<PhotosOptions> options,
    ILogger<PhotoUploadService> logger
) : IPhotoUploadService
{
    private readonly PhotosOptions _options = options.Value;

    public async Task<UploadUrlResponse> CreateUploadUrlAsync(
        UploadUrlRequest request,
        CancellationToken ct
    )
    {
        var key = $"photos/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}.jpg";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.PresignedUrlTtlMinutes);

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = request.ContentType,
        };

        var url = await s3.GetPreSignedURLAsync(presignRequest);

        logger.LogInformation("Issued upload URL for key {Key} expiring at {ExpiresAt}", key, expiresAt);

        return new UploadUrlResponse(url, key, expiresAt);
    }
}

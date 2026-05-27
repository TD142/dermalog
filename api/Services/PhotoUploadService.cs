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

    public async Task<ServiceResult<UploadUrlResponse>> CreateUploadUrlAsync(
        UploadUrlRequest request,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            return ServiceResult<UploadUrlResponse>.Failure(
                ServiceResultError.Validation,
                "contentType is required"
            );
        }

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

        try
        {
            var url = await s3.GetPreSignedURLAsync(presignRequest);

            logger.LogInformation(
                "Issued upload URL for key {Key} expiring at {ExpiresAt}",
                key,
                expiresAt
            );

            return ServiceResult<UploadUrlResponse>.Success(
                new UploadUrlResponse(url, key, expiresAt)
            );
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "S3 rejected pre-sign request for key {Key}", key);
            return ServiceResult<UploadUrlResponse>.Failure(
                ServiceResultError.External,
                "Failed to issue upload URL"
            );
        }
    }
}

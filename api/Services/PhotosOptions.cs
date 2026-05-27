namespace Dermalog.Api.Services;

public class PhotosOptions
{
    public required string BucketName { get; init; }
    public int PresignedUrlTtlMinutes { get; init; } = 15;
}

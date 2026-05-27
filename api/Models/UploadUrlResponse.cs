namespace Dermalog.Api.Models;

public record UploadUrlResponse(
    string UploadUrl,
    string ObjectKey,
    DateTimeOffset ExpiresAt
);

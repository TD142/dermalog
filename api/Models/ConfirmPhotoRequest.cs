namespace Dermalog.Api.Models;

public record ConfirmPhotoRequest(string ObjectKey, string ContentType, DateTimeOffset? CapturedAt);

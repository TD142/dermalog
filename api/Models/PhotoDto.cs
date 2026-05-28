namespace Dermalog.Api.Models;

public record PhotoDto(
    Guid Id,
    string ObjectKey,
    string ContentType,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    string Url,
    DateTimeOffset UrlExpiresAt
);

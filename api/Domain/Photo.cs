namespace Dermalog.Api.Domain;

public class Photo
{
    public Guid Id { get; private set; }
    public string ObjectKey { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public DateTimeOffset CapturedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Photo() { }

    public static Photo Create(string objectKey, string contentType, DateTimeOffset capturedAt)
    {
        return new Photo
        {
            Id = Guid.NewGuid(),
            ObjectKey = objectKey,
            ContentType = contentType,
            CapturedAt = capturedAt,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}

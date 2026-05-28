using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dermalog.Api.Services;

public class PhotoService(DermalogDbContext db, ILogger<PhotoService> logger) : IPhotoService
{
    private const int MaxTake = 200;
    private const int DefaultTake = 50;

    public async Task<ServiceResult<PhotoDto>> ConfirmAsync(
        ConfirmPhotoRequest request,
        CancellationToken ct
    )
    {
        var existing = await db.Photos.AnyAsync(p => p.ObjectKey == request.ObjectKey, ct);
        if (existing)
        {
            return ServiceResult<PhotoDto>.Failure(
                ServiceResultError.Conflict,
                "Photo with that objectKey already confirmed"
            );
        }

        var photo = Photo.Create(
            request.ObjectKey,
            request.ContentType,
            request.CapturedAt ?? DateTimeOffset.UtcNow
        );

        db.Photos.Add(photo);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Confirmed photo {Id} with objectKey {ObjectKey}",
            photo.Id,
            photo.ObjectKey
        );

        return ServiceResult<PhotoDto>.Success(MapToDto(photo));
    }

    public async Task<ServiceResult<IReadOnlyList<PhotoDto>>> ListAsync(
        int take,
        CancellationToken ct
    )
    {
        var effectiveTake = take <= 0 ? DefaultTake : Math.Min(take, MaxTake);

        var photos = await db
            .Photos.AsNoTracking()
            .OrderByDescending(p => p.CapturedAt)
            .Take(effectiveTake)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);

        return ServiceResult<IReadOnlyList<PhotoDto>>.Success(photos);
    }

    private static PhotoDto MapToDto(Photo p) =>
        new(p.Id, p.ObjectKey, p.ContentType, p.CapturedAt, p.CreatedAt);
}

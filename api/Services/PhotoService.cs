using Amazon.S3;
using Dermalog.Api.Data;
using Dermalog.Api.Domain;
using Dermalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dermalog.Api.Services;

public class PhotoService(
    DermalogDbContext db,
    IPhotoStorageService storage,
    IOptions<PhotosOptions> options,
    ILogger<PhotoService> logger
) : IPhotoService
{
    private const int MaxTake = 200;
    private const int DefaultTake = 50;

    private readonly PhotosOptions _options = options.Value;

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

        var dto = await MapToDtoAsync(photo, ct);
        return ServiceResult<PhotoDto>.Success(dto);
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
            .ToListAsync(ct);

        var dtos = new List<PhotoDto>(photos.Count);
        foreach (var photo in photos)
        {
            dtos.Add(await MapToDtoAsync(photo, ct));
        }

        return ServiceResult<IReadOnlyList<PhotoDto>>.Success(dtos);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var photo = await db.Photos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (photo is null)
        {
            return ServiceResult<bool>.Failure(ServiceResultError.NotFound, "Photo not found");
        }

        var referenced = await db.Comparisons.AnyAsync(
            c => c.BeforePhotoId == id || c.AfterPhotoId == id,
            ct
        );
        if (referenced)
        {
            return ServiceResult<bool>.Failure(
                ServiceResultError.Conflict,
                "Photo is used in a comparison and can't be deleted"
            );
        }

        try
        {
            await storage.DeleteObjectAsync(photo.ObjectKey, ct);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "Failed to delete S3 object {Key}", photo.ObjectKey);
            return ServiceResult<bool>.Failure(
                ServiceResultError.External,
                "Failed to delete photo from storage"
            );
        }

        db.Photos.Remove(photo);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted photo {Id}", id);
        return ServiceResult<bool>.Success(true);
    }

    private async Task<PhotoDto> MapToDtoAsync(Photo p, CancellationToken ct)
    {
        var urlExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.PresignedUrlTtlMinutes);
        var url = await storage.CreateDownloadUrlAsync(p.ObjectKey, urlExpiresAt, ct);
        return new PhotoDto(
            p.Id,
            p.ObjectKey,
            p.ContentType,
            p.CapturedAt,
            p.CreatedAt,
            url,
            urlExpiresAt
        );
    }
}

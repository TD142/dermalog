using Dermalog.Api.Models;
using Dermalog.Api.Services;
using Dermalog.Api.Services.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers.Photos;

[ApiController]
[Route("api/v1/photos")]
public class PhotosController(
    IPhotoUploadService uploadService,
    IPhotoService photoService,
    IPhotoComparisonService comparisonService
) : ControllerBase
{
    [HttpPost("upload-url")]
    public async Task<Results<Ok<UploadUrlResponse>, ProblemHttpResult>> CreateUploadUrl(
        UploadUrlRequest request,
        CancellationToken ct
    )
    {
        var result = await uploadService.CreateUploadUrlAsync(request, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpPost]
    public async Task<Results<Created<PhotoDto>, ProblemHttpResult>> Confirm(
        ConfirmPhotoRequest request,
        CancellationToken ct
    )
    {
        var result = await photoService.ConfirmAsync(request, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/photos/{result.Value!.Id}", result.Value)
            : result.ToProblem();
    }

    [HttpGet]
    public async Task<Results<Ok<IReadOnlyList<PhotoDto>>, ProblemHttpResult>> List(
        [FromQuery] int take = 50,
        CancellationToken ct = default
    )
    {
        var result = await photoService.ListAsync(take, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpPost("compare")]
    public async Task<Results<Ok<ComparisonResult>, ProblemHttpResult>> Compare(
        ComparePhotosRequest request,
        CancellationToken ct
    )
    {
        var result = await comparisonService.CompareAsync(request, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }
}

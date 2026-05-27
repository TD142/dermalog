using Dermalog.Api.Models;
using Dermalog.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers.Photos;

[ApiController]
[Route("api/v1/photos")]
public class PhotosController(IPhotoUploadService uploadService) : ControllerBase
{
    [HttpPost("upload-url")]
    public async Task<Results<Ok<UploadUrlResponse>, ProblemHttpResult>> CreateUploadUrl(
        UploadUrlRequest request,
        CancellationToken ct
    )
    {
        var result = await uploadService.CreateUploadUrlAsync(request, ct);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        var status = result.ErrorKind switch
        {
            ServiceResultError.Validation => StatusCodes.Status400BadRequest,
            ServiceResultError.NotFound => StatusCodes.Status404NotFound,
            ServiceResultError.Conflict => StatusCodes.Status409Conflict,
            ServiceResultError.External => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError,
        };

        return TypedResults.Problem(detail: result.Reason, statusCode: status);
    }
}

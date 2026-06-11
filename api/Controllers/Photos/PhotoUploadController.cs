using Dermalog.Api.Models;
using Dermalog.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers.Photos;

[ApiController]
[Route("api/v1/photos")]
public class PhotoUploadController(IPhotoStorageService uploadService) : ControllerBase
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
}

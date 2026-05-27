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
    public async Task<Ok<UploadUrlResponse>> CreateUploadUrl(
        UploadUrlRequest request,
        CancellationToken ct
    )
    {
        var response = await uploadService.CreateUploadUrlAsync(request, ct);
        return TypedResults.Ok(response);
    }
}

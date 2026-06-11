using Dermalog.Api.Models;
using Dermalog.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers.Photos;

[ApiController]
[Route("api/v1/photos")]
public class PhotosController(IPhotoService photoService) : ControllerBase
{
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

    [HttpDelete("{id:guid}")]
    public async Task<Results<NoContent, ProblemHttpResult>> Delete(Guid id, CancellationToken ct)
    {
        var result = await photoService.DeleteAsync(id, ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }
}

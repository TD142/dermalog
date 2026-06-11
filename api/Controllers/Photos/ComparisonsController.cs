using Dermalog.Api.Models;
using Dermalog.Api.Services.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers.Photos;

[ApiController]
[Route("api/v1/photos")]
public class ComparisonsController(IPhotoComparisonService comparisonService) : ControllerBase
{
    [HttpPost("compare")]
    public async Task<Results<Ok<ComparisonDto>, ProblemHttpResult>> Compare(
        ComparePhotosRequest request,
        CancellationToken ct
    )
    {
        var result = await comparisonService.CompareAsync(request, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpGet("comparisons/latest")]
    public async Task<Results<Ok<ComparisonDto>, NoContent, ProblemHttpResult>> GetLatest(
        CancellationToken ct
    )
    {
        var result = await comparisonService.GetLatestAsync(ct);
        if (!result.IsSuccess)
        {
            return result.ToProblem();
        }

        return result.Value is null ? TypedResults.NoContent() : TypedResults.Ok(result.Value);
    }

    [HttpGet("comparisons")]
    public async Task<Results<Ok<IReadOnlyList<ComparisonDto>>, ProblemHttpResult>> List(
        CancellationToken ct
    )
    {
        var result = await comparisonService.GetAllAsync(ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpPatch("comparisons/{id:guid}")]
    public async Task<Results<Ok<ComparisonDto>, ProblemHttpResult>> Update(
        Guid id,
        UpdateComparisonRequest request,
        CancellationToken ct
    )
    {
        var result = await comparisonService.UpdateAsync(id, request.Label, request.IsComplete, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpDelete("comparisons/{id:guid}")]
    public async Task<Results<NoContent, ProblemHttpResult>> Delete(Guid id, CancellationToken ct)
    {
        var result = await comparisonService.DeleteAsync(id, ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }
}

using Dermalog.Api.Models;
using Dermalog.Api.Services.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers;

[ApiController]
[Route("api/v1/insight")]
public class InsightController(IInsightService insightService) : ControllerBase
{
    [HttpGet]
    public async Task<Results<Ok<InsightDto>, NoContent, ProblemHttpResult>> Get(
        CancellationToken ct
    )
    {
        var result = await insightService.GetAsync(ct);
        if (!result.IsSuccess)
        {
            return result.ToProblem();
        }

        return result.Value is null ? TypedResults.NoContent() : TypedResults.Ok(result.Value);
    }
}

using Dermalog.Api.Models;
using Dermalog.Api.Services.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dermalog.Api.Controllers;

[ApiController]
[Route("api/v1/journal")]
public class JournalController(IJournalService journalService) : ControllerBase
{
    [HttpPost]
    public async Task<Results<Created<JournalEntryDto>, ProblemHttpResult>> Create(
        CreateJournalEntryRequest request,
        CancellationToken ct
    )
    {
        var result = await journalService.CreateAsync(request.Text, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/journal/{result.Value!.Id}", result.Value)
            : result.ToProblem();
    }

    [HttpGet]
    public async Task<Results<Ok<IReadOnlyList<JournalEntryDto>>, ProblemHttpResult>> List(
        CancellationToken ct
    )
    {
        var result = await journalService.GetAllAsync(ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value!) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    public async Task<Results<NoContent, ProblemHttpResult>> Delete(Guid id, CancellationToken ct)
    {
        var result = await journalService.DeleteAsync(id, ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }
}

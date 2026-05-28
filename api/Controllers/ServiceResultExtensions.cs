using Dermalog.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dermalog.Api.Controllers;

public static class ServiceResultExtensions
{
    public static ProblemHttpResult ToProblem<T>(this ServiceResult<T> result) =>
        TypedResults.Problem(
            detail: result.Reason,
            statusCode: result.ErrorKind switch
            {
                ServiceResultError.Validation => StatusCodes.Status400BadRequest,
                ServiceResultError.NotFound => StatusCodes.Status404NotFound,
                ServiceResultError.Conflict => StatusCodes.Status409Conflict,
                ServiceResultError.External => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError,
            }
        );
}

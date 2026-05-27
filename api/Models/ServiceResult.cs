namespace Dermalog.Api.Models;

public enum ServiceResultError
{
    Validation,
    NotFound,
    Conflict,
    External,
    Unexpected,
}

public record ServiceResult<T>(
    bool IsSuccess,
    T? Value,
    ServiceResultError? ErrorKind,
    string? Reason
)
{
    public static ServiceResult<T> Success(T value) => new(true, value, null, null);

    public static ServiceResult<T> Failure(ServiceResultError kind, string reason) =>
        new(false, default, kind, reason);
}

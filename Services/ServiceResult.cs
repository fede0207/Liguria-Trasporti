using Liguria_Trasporti.Enums;

namespace Liguria_Trasporti.Services;

public sealed class ServiceResult<T>
{
    public bool Success { get; init; }
    public ServiceResultStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T>()
        {
            Success = true,
            Status = ServiceResultStatus.Success,
            Data = data
        };
    }

    public static ServiceResult<T> NotFound(T data)
    {
        return new ServiceResult<T>()
        {
            Success = false,
            ErrorMessage = "Resource Not Found",
            Status = ServiceResultStatus.NotFound,
            Data = data
        };
    }

    public static ServiceResult<T> ValidationError()
    {
        return new ServiceResult<T>()
        {
            Success = false,
            ErrorMessage = "Input validation error",
            Status = ServiceResultStatus.ValidationError,
        };
    }

    public static ServiceResult<T> Conflict(T data)
    {
        return new ServiceResult<T>()
        {
            Success = false,
            ErrorMessage = "Conflict",
            Status = ServiceResultStatus.Conflict,
            Data = data
        };
    }

    public static ServiceResult<T> Unauthorized(T data)
    {
        return new ServiceResult<T>()
        {
            Success = false,
            ErrorMessage = "Unauthorized",
            Status = ServiceResultStatus.Unauthorized,
            Data = data
        };
    }

    public static ServiceResult<T> Forbidden(T data)
    {
        return new ServiceResult<T>()
        {
            Success = false,
            ErrorMessage = "Forbidden",
            Status = ServiceResultStatus.Forbidden,
            Data = data
        };
    }
}
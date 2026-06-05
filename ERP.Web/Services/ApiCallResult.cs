namespace ERP.Web.Services;

public sealed class ApiCallResult<T>
{
    public bool IsSuccess { get; init; }
    public int? StatusCode { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiCallResult<T> Success(T? data, int? statusCode = null)
        => new()
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data
        };

    public static ApiCallResult<T> Failure(string? errorMessage, int? statusCode = null)
        => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage
        };
}


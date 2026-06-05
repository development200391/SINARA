namespace ERP.Web.Services;

public static class ApiCallResultTaskExtensions
{
    public static async Task<T?> ToDataAsync<T>(this Task<ApiCallResult<T>> resultTask)
    {
        var result = await resultTask;
        return result.Data;
    }
}

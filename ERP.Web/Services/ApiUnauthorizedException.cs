namespace ERP.Web.Services;

public sealed class ApiUnauthorizedException(string? endpoint = null)
    : Exception(string.IsNullOrWhiteSpace(endpoint)
        ? "API returned 401 Unauthorized."
        : $"API returned 401 Unauthorized for endpoint '{endpoint}'.")
{
}

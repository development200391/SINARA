using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ERP.Web.Services;

public abstract class ApiClientBase(HttpClient httpClient, ILogger logger, string serviceName)
{

    protected async Task<ApiCallResult<T>> SendWithResultAsync<T>(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        using var response = await SendRawAsync(method, uri, accessToken, body, ct);
        if (response is null)
        {
            return ApiCallResult<T>.Failure($"Failed to call {serviceName} API endpoint {uri}.");
        }

        var statusCode = (int)response.StatusCode;
        if (!response.IsSuccessStatusCode)
        {
            var apiErrorMessage = await ReadApiErrorMessageAsync(response, ct);
            var errorMessage = string.IsNullOrWhiteSpace(apiErrorMessage)
                ? $"{serviceName} API returned status {statusCode} ({response.StatusCode})."
                : $"{serviceName} API returned status {statusCode} ({response.StatusCode}): {apiErrorMessage}";

            return ApiCallResult<T>.Failure(errorMessage, statusCode);
        }

        if (response.Content is null || response.Content.Headers.ContentLength == 0)
        {
            return ApiCallResult<T>.Success(default, statusCode);
        }

        try
        {
            var responseBody = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            return ApiCallResult<T>.Success(responseBody, statusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize {ServiceName} API response from {Uri}", serviceName, uri);
            return ApiCallResult<T>.Failure($"Failed to deserialize {serviceName} API response from {uri}.", statusCode);
        }
    }

    protected async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException(uri);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call {ServiceName} API endpoint {Uri}", serviceName, uri);
            return null;
        }
    }

    private static async Task<string?> ReadApiErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content is null)
        {
            return null;
        }

        string rawContent;
        try
        {
            rawContent = await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        var trimmedContent = rawContent.Trim();
        try
        {
            using var document = JsonDocument.Parse(trimmedContent);
            var parsedMessage = BuildApiErrorMessage(document.RootElement);
            if (!string.IsNullOrWhiteSpace(parsedMessage))
            {
                return parsedMessage;
            }
        }
        catch (JsonException)
        {
        }

        return trimmedContent;
    }

    private static string? BuildApiErrorMessage(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var segments = new List<string>();

        AppendIfHasValue(root, segments, "message");
        AppendIfHasValue(root, segments, "error");
        AppendIfHasValue(root, segments, "title");
        AppendIfHasValue(root, segments, "detail");

        if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var entry in errorsElement.EnumerateObject())
            {
                var value = entry.Value.ValueKind switch
                {
                    JsonValueKind.String => entry.Value.GetString(),
                    JsonValueKind.Array => string.Join(", ", entry.Value.EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.String)
                        .Select(item => item.GetString())
                        .Where(item => !string.IsNullOrWhiteSpace(item))),
                    _ => entry.Value.ToString()
                };

                if (!string.IsNullOrWhiteSpace(value))
                {
                    segments.Add($"{entry.Name}: {value}");
                }
            }
        }

        if (segments.Count == 0)
        {
            return null;
        }

        return string.Join(" | ", segments.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static void AppendIfHasValue(JsonElement root, List<string> segments, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(property.GetString()))
        {
            segments.Add(property.GetString()!);
        }
    }
}

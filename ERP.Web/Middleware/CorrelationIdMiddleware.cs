namespace ERP.Web.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderKey = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items[HeaderKey] = correlationId;
        context.Response.Headers[HeaderKey] = correlationId;

        await next(context);
    }
}

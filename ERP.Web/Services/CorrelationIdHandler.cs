using ERP.Web.Middleware;

namespace ERP.Web.Services;

public sealed class CorrelationIdHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(CorrelationIdMiddleware.HeaderKey, out var correlationId) == true && correlationId is string id)
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderKey, id);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

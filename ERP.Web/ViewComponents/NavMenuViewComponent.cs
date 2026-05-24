using System.Security.Claims;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class NavMenuViewComponent(IConfigApiClient configApiClient) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View(Enumerable.Empty<NavigationModuleDto>());
        }

        var accessToken = UserClaimsPrincipal?.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return View(Enumerable.Empty<NavigationModuleDto>());
        }

        var modules = await configApiClient.GetNavigationAsync(accessToken, ct);
        return View(modules);
    }
}

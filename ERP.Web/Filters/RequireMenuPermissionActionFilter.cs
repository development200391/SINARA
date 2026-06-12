using System.Security.Claims;
using ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Web.Filters;

public sealed class RequireMenuPermissionActionFilter(
    IMenuPermissionService menuPermissionService,
    string menuUrl,
    string? menuKey,
    MenuPermissionAction action) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var accessToken = context.HttpContext.User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var returnUrl = $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
            return;
        }

        var permission = await menuPermissionService.GetMenuPermissionAsync(
            context.HttpContext.User,
            accessToken,
            menuUrl,
            menuKey,
            context.HttpContext.RequestAborted);

        if (!permission.Allows(action))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            return;
        }

        await next();
    }
}

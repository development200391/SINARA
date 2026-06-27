using System.Security.Claims;
using ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Web.Filters;

public sealed class AutoRequireMenuPermissionFilter(IMenuPermissionService menuPermissionService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (ShouldSkip(context))
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        var accessToken = user.FindFirstValue("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var returnUrl = $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
            return;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var requiredAction = ResolveRequiredAction(context);

        var result = await menuPermissionService.GetMenuPermissionResultAsync(
            user,
            accessToken,
            path,
            null,
            context.HttpContext.RequestAborted);

        if (!result.IsMenuMatched)
        {
            if (IsFailClosedPath(path))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            await next();
            return;
        }

        if (!result.Permission.Allows(requiredAction))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            return;
        }

        await next();
    }

    private static bool IsFailClosedPath(string path)
    {
        return path.StartsWith("/sales", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldSkip(ActionExecutingContext context)
    {
        if (context.Filters.Any(x => x is IAllowAnonymousFilter))
        {
            return true;
        }

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        if (path.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/language", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor
            && string.Equals(actionDescriptor.ControllerName, "Home", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static MenuPermissionAction ResolveRequiredAction(ActionExecutingContext context)
    {
        var actionName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName ?? string.Empty;
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var hint = $"{actionName} {path}".ToLowerInvariant();

        if (ContainsAny(hint, ["create", "add", "new"]))
        {
            return MenuPermissionAction.Create;
        }

        if (ContainsAny(hint, ["delete", "remove"]))
        {
            return MenuPermissionAction.Delete;
        }

        if (ContainsAny(hint, ["edit", "update", "approve", "reject", "revoke", "process", "confirm", "cancel", "toggle", "run", "send", "close", "open", "start", "complete", "post"]))
        {
            return MenuPermissionAction.Edit;
        }

        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsDelete(method))
        {
            return MenuPermissionAction.Delete;
        }

        if (HttpMethods.IsGet(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method)
            || HttpMethods.IsTrace(method))
        {
            return MenuPermissionAction.View;
        }

        return MenuPermissionAction.Edit;
    }

    private static bool ContainsAny(string source, IReadOnlyList<string> tokens)
    {
        foreach (var token in tokens)
        {
            if (source.Contains(token, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

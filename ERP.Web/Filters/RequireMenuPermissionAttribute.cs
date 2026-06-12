using ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireMenuPermissionAttribute : TypeFilterAttribute
{
    public RequireMenuPermissionAttribute(string menuUrl, MenuPermissionAction action, string? menuKey = null)
        : base(typeof(RequireMenuPermissionActionFilter))
    {
        Arguments = new object[] { menuUrl, menuKey ?? string.Empty, action };
    }
}


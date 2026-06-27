using System.Security.Claims;
using ERP.Web.Services;
using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class PagedGridViewComponent(IMenuPermissionService menuPermissionService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(PagedGridViewModel model)
    {
        var user = UserClaimsPrincipal;
        var accessToken = user?.FindFirstValue("access_token");

        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(accessToken))
        {
            return View(model);
        }

        var permissionCache = new Dictionary<string, MenuPermissionResult>(StringComparer.OrdinalIgnoreCase);
        var ct = HttpContext.RequestAborted;

        async Task<bool> IsAllowedAsync(string? actionUrl, MenuPermissionAction action)
        {
            if (string.IsNullOrWhiteSpace(actionUrl))
            {
                return false;
            }

            if (!permissionCache.TryGetValue(actionUrl, out var result))
            {
                result = await menuPermissionService.GetMenuPermissionResultAsync(
                    user,
                    accessToken,
                    actionUrl,
                    null,
                    ct);

                permissionCache[actionUrl] = result;
            }

            if (!result.IsMenuMatched)
            {
                return !IsFailClosedActionUrl(actionUrl);
            }

            return result.Permission.Allows(action);
        }

        static bool IsFailClosedActionUrl(string actionUrl)
        {
            var candidate = actionUrl.Trim();

            if (Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteUri))
            {
                candidate = absoluteUri.PathAndQuery;
            }

            var queryOrHashIndex = candidate.IndexOfAny(['?', '#']);
            if (queryOrHashIndex >= 0)
            {
                candidate = candidate[..queryOrHashIndex];
            }

            if (!candidate.StartsWith("/", StringComparison.Ordinal))
            {
                candidate = $"/{candidate}";
            }

            return candidate.StartsWith("/sales", StringComparison.OrdinalIgnoreCase);
        }

        var securedRows = new List<PagedGridRowViewModel>(model.Rows.Count);
        foreach (var row in model.Rows)
        {
            var rowActions = row.Actions;
            if (rowActions is null)
            {
                securedRows.Add(row);
                continue;
            }

            var canDetail = await IsAllowedAsync(rowActions.DetailUrl, MenuPermissionAction.View);
            var canEdit = await IsAllowedAsync(rowActions.EditUrl, MenuPermissionAction.Edit);
            var canApprove = await IsAllowedAsync(rowActions.ApproveUrl, MenuPermissionAction.Edit);
            var canReject = await IsAllowedAsync(rowActions.RejectUrl, MenuPermissionAction.Edit);
            var canDelete = await IsAllowedAsync(rowActions.DeleteUrl, MenuPermissionAction.Delete);

            securedRows.Add(new PagedGridRowViewModel
            {
                Cells = row.Cells,
                Actions = new PagedGridRowActionViewModel
                {
                    DetailUrl = canDetail ? rowActions.DetailUrl : null,
                    EditUrl = canEdit ? rowActions.EditUrl : null,
                    ApproveUrl = canApprove ? rowActions.ApproveUrl : null,
                    RejectUrl = canReject ? rowActions.RejectUrl : null,
                    DeleteUrl = canDelete ? rowActions.DeleteUrl : null,
                    ApproveConfirmMessage = rowActions.ApproveConfirmMessage,
                    RejectConfirmMessage = rowActions.RejectConfirmMessage,
                    DeleteConfirmMessage = rowActions.DeleteConfirmMessage
                }
            });
        }

        var canAdd = await IsAllowedAsync(model.Actions.AddUrl, MenuPermissionAction.Create);

        var securedModel = new PagedGridViewModel
        {
            Columns = model.Columns,
            Rows = securedRows,
            EmptyMessage = model.EmptyMessage,
            Page = model.Page,
            TotalPages = model.TotalPages,
            PageSize = model.PageSize,
            PageSizeOptions = model.PageSizeOptions,
            SortBy = model.SortBy,
            SortDirection = model.SortDirection,
            EnableSearch = model.EnableSearch,
            SearchQueryKey = model.SearchQueryKey,
            SearchValue = model.SearchValue,
            SearchPlaceholder = model.SearchPlaceholder,
            SearchButtonText = model.SearchButtonText,
            ExportUrl = model.ExportUrl,
            ExportButtonText = model.ExportButtonText,
            Actions = new PagedGridActionConfigViewModel
            {
                ActionColumnTitle = model.Actions.ActionColumnTitle,
                AddUrl = canAdd ? model.Actions.AddUrl : null,
                AddButtonText = model.Actions.AddButtonText,
                DetailButtonText = model.Actions.DetailButtonText,
                EditButtonText = model.Actions.EditButtonText,
                ApproveButtonText = model.Actions.ApproveButtonText,
                RejectButtonText = model.Actions.RejectButtonText,
                DeleteButtonText = model.Actions.DeleteButtonText,
                ApproveConfirmMessage = model.Actions.ApproveConfirmMessage,
                RejectConfirmMessage = model.Actions.RejectConfirmMessage,
                DeleteConfirmMessage = model.Actions.DeleteConfirmMessage
            },
            RouteValues = model.RouteValues,
            PageQueryKey = model.PageQueryKey,
            PageSizeQueryKey = model.PageSizeQueryKey,
            SortByQueryKey = model.SortByQueryKey,
            SortDirectionQueryKey = model.SortDirectionQueryKey
        };

        return View(securedModel);
    }
}

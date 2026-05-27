using System.Runtime.CompilerServices;
using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.Services.Exports;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/audit")]
public sealed class ConfigAuditController(
    IConfigApiClient configApiClient,
    IAuditLogExcelExportService auditLogExcelExportService) : Controller
{
    private const int DefaultPageSize = 20;
    private const int ExportPageSize = 200;
    private const string DefaultSortBy = "createdAt";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "desc",
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var logs = await configApiClient.GetAuditLogsAsync(accessToken, new PagedRequest
        {
            Page = page,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection
        }, ct);

        ViewData["Title"] = "Audit Log";
        ViewData["Breadcrumb"] = "Configuration / Audit Log";

        return View(new ConfigAuditIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Logs = logs ?? PagedResult<AuditLogDto>.Create([], 0, page, normalizedPageSize)
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "desc",
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var tempPath = Path.Combine(Path.GetTempPath(), $"sinara-audit-{Guid.NewGuid():N}.xlsx");

        try
        {
            var rows = GetAuditRowsForExportAsync(accessToken, search, normalizedSortBy, normalizedSortDirection, ct);
            await auditLogExcelExportService.WriteAsync(tempPath, rows, ct);

            HttpContext.Response.OnCompleted(() =>
            {
                try
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                    }
                }
                catch
                {
                    // Swallow cleanup errors.
                }

                return Task.CompletedTask;
            });

            var downloadName = $"audit-log-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.xlsx";
            var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", downloadName);
        }
        catch
        {
            try
            {
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
            catch
            {
                // Swallow cleanup errors.
            }

            throw;
        }
    }

    private async IAsyncEnumerable<AuditLogDto> GetAuditRowsForExportAsync(
        string accessToken,
        string? search,
        string sortBy,
        string sortDirection,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var page = 1;

        while (true)
        {
            var result = await configApiClient.GetAuditLogsAsync(accessToken, new PagedRequest
            {
                Page = page,
                PageSize = ExportPageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection
            }, ct);

            if (result is null)
            {
                throw new InvalidOperationException("Failed to fetch audit logs for export.");
            }

            if (result.Items.Count == 0)
            {
                yield break;
            }

            foreach (var row in result.Items)
            {
                yield return row;
            }

            if (result.Page >= result.TotalPages)
            {
                yield break;
            }

            page++;
        }
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private static string NormalizeSortBy(string? sortBy) => string.IsNullOrWhiteSpace(sortBy) ? DefaultSortBy : sortBy.Trim();

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}

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
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        string? status = null,
        bool hasIpOnly = false,
        string[]? entityNames = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);
        var normalizedStatus = NormalizeStatus(status);
        var normalizedEntityNames = NormalizeMultiSelectValues(entityNames);

        var logs = await configApiClient.GetAuditLogsAsync(accessToken, new AuditLogPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            Status = normalizedStatus,
            HasIpOnly = hasIpOnly,
            EntityNames = normalizedEntityNames.Count > 0
                ? string.Join(",", normalizedEntityNames)
                : null
        }, ct);

        var safeLogs = logs ?? PagedResult<AuditLogDto>.Create([], 0, normalizedPage, normalizedPageSize);
        var statusOptions = BuildDistinctOptions(
            safeLogs.Items.Select(x => x.Action),
            string.IsNullOrWhiteSpace(normalizedStatus) ? [] : [normalizedStatus]);
        var entityNameOptions = BuildDistinctOptions(
            safeLogs.Items.Select(x => x.EntityName),
            normalizedEntityNames);

        ViewData["Title"] = "Audit Log";
        ViewData["Breadcrumb"] = "Configuration / Audit Log";

        return View(new ConfigAuditIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            Status = normalizedStatus,
            HasIpOnly = hasIpOnly,
            SelectedEntityNames = normalizedEntityNames,
            StatusOptions = statusOptions,
            EntityNameOptions = entityNameOptions,
            Logs = safeLogs
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "desc",
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        string? status = null,
        bool hasIpOnly = false,
        string[]? entityNames = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);
        var normalizedStatus = NormalizeStatus(status);
        var normalizedEntityNames = NormalizeMultiSelectValues(entityNames);
        var tempPath = Path.Combine(Path.GetTempPath(), $"sinara-audit-{Guid.NewGuid():N}.xlsx");

        try
        {
            var rows = GetAuditRowsForExportAsync(
                accessToken,
                search,
                normalizedSortBy,
                normalizedSortDirection,
                normalizedDateFrom,
                normalizedDateTo,
                normalizedStatus,
                hasIpOnly,
                normalizedEntityNames,
                ct);

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
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? status,
        bool hasIpOnly,
        IReadOnlyList<string> entityNames,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var page = 1;

        while (true)
        {
            var result = await configApiClient.GetAuditLogsAsync(accessToken, new AuditLogPagedRequest
            {
                Page = page,
                PageSize = ExportPageSize,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                HasIpOnly = hasIpOnly,
                EntityNames = entityNames.Count > 0 ? string.Join(",", entityNames) : null
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

    private static string? NormalizeStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? null : status.Trim();

    private static (DateOnly? DateFrom, DateOnly? DateTo) NormalizeDateRange(DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
        {
            return (dateTo, dateFrom);
        }

        return (dateFrom, dateTo);
    }

    private static IReadOnlyList<string> NormalizeMultiSelectValues(string[]? values)
    {
        if (values is null || values.Length == 0)
        {
            return [];
        }

        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .SelectMany(x => x.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> BuildDistinctOptions(IEnumerable<string?> values, IEnumerable<string> selectedValues)
    {
        var options = values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var selectedValue in selectedValues)
        {
            if (options.Contains(selectedValue, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            options.Add(selectedValue);
        }

        return options
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}

using System.Globalization;
using System.Text;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("reports/trial-balance")]
    public async Task<IActionResult> TrialBalance(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "accountcode",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? accountId = null,
        int? costCenterId = null,
        FinanceAccountType? type = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "accountcode", "accountcode", "accountname", "type", "totaldebit", "totalcredit", "balance", "endingdebit", "endingcredit");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = financeApiClient.GetTrialBalanceAsync(accessToken, new TrialBalancePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodId = periodId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            AccountId = accountId,
            CostCenterId = costCenterId,
            Type = type
        }, ct);

        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, periodOptionsTask, accountOptionsTask, costCenterOptionsTask);

        ViewData["Title"] = "Trial Balance";
        ViewData["Breadcrumb"] = "Finance / Financial Reports / Trial Balance";

        return View("Reports/TrialBalance", new FinanceTrialBalanceIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodIdFilter = periodId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            AccountIdFilter = accountId,
            CostCenterIdFilter = costCenterId,
            TypeFilter = type,
            PeriodOptions = await periodOptionsTask,
            AccountOptions = await accountOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            Items = await itemsTask ?? PagedResult<TrialBalanceRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/trial-balance/export")]
    public async Task<IActionResult> ExportTrialBalance(
        string? search = null,
        string? sortBy = "accountcode",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? accountId = null,
        int? costCenterId = null,
        FinanceAccountType? type = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedSortBy = NormalizeSortBy(sortBy, "accountcode", "accountcode", "accountname", "type", "totaldebit", "totalcredit", "balance", "endingdebit", "endingcredit");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var rows = await LoadAllTrialBalanceRowsAsync(accessToken, new TrialBalancePagedRequest
        {
            Page = 1,
            PageSize = 500,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodId = periodId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            AccountId = accountId,
            CostCenterId = costCenterId,
            Type = type
        }, ct);

        var csvRows = rows.Select(x => (IReadOnlyList<string>)
        [
            x.AccountCode,
            x.AccountName,
            x.AccountType.ToString(),
            x.TotalDebit.ToString("0.00", CultureInfo.InvariantCulture),
            x.TotalCredit.ToString("0.00", CultureInfo.InvariantCulture),
            x.EndingDebit.ToString("0.00", CultureInfo.InvariantCulture),
            x.EndingCredit.ToString("0.00", CultureInfo.InvariantCulture)
        ]);

        return BuildCsvFile(
            "trial-balance",
            ["Account Code", "Account Name", "Type", "Total Debit", "Total Credit", "Ending Debit", "Ending Credit"],
            csvRows);
    }

    [HttpGet("reports/balance-sheet")]
    public Task<IActionResult> BalanceSheet(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        var sections = new[] { "Asset", "Liability", "Equity" };
        var allowedTypes = new[] { FinanceAccountType.Asset, FinanceAccountType.Liability, FinanceAccountType.Equity };

        return RenderFinancialStatementAsync(
            reportTitle: "Balance Sheet",
            reportKey: "balance-sheet",
            exportActionName: nameof(ExportBalanceSheet),
            fetcher: (token, request, tokenCt) => financeApiClient.GetBalanceSheetAsync(token, request, tokenCt),
            sections: sections,
            allowedAccountTypes: allowedTypes,
            page: page,
            pageSize: pageSize,
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    [HttpGet("reports/profit-loss")]
    public Task<IActionResult> ProfitLoss(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        var sections = new[] { "Revenue", "Expense" };
        var allowedTypes = new[] { FinanceAccountType.Revenue, FinanceAccountType.Expense };

        return RenderFinancialStatementAsync(
            reportTitle: "Profit & Loss",
            reportKey: "profit-loss",
            exportActionName: nameof(ExportProfitLoss),
            fetcher: (token, request, tokenCt) => financeApiClient.GetProfitLossAsync(token, request, tokenCt),
            sections: sections,
            allowedAccountTypes: allowedTypes,
            page: page,
            pageSize: pageSize,
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    [HttpGet("reports/cash-flow")]
    public Task<IActionResult> CashFlow(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        var sections = new[] { "Operating", "Investing", "Financing" };
        var allowedTypes = new[]
        {
            FinanceAccountType.Asset,
            FinanceAccountType.Liability,
            FinanceAccountType.Equity,
            FinanceAccountType.Revenue,
            FinanceAccountType.Expense
        };

        return RenderFinancialStatementAsync(
            reportTitle: "Cash Flow",
            reportKey: "cash-flow",
            exportActionName: nameof(ExportCashFlow),
            fetcher: (token, request, tokenCt) => financeApiClient.GetCashFlowAsync(token, request, tokenCt),
            sections: sections,
            allowedAccountTypes: allowedTypes,
            page: page,
            pageSize: pageSize,
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    [HttpGet("reports/balance-sheet/export")]
    public Task<IActionResult> ExportBalanceSheet(
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        return ExportFinancialStatementAsync(
            filePrefix: "balance-sheet",
            fetcher: (token, request, tokenCt) => financeApiClient.GetBalanceSheetAsync(token, request, tokenCt),
            sections: ["Asset", "Liability", "Equity"],
            allowedAccountTypes: [FinanceAccountType.Asset, FinanceAccountType.Liability, FinanceAccountType.Equity],
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    [HttpGet("reports/profit-loss/export")]
    public Task<IActionResult> ExportProfitLoss(
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        return ExportFinancialStatementAsync(
            filePrefix: "profit-loss",
            fetcher: (token, request, tokenCt) => financeApiClient.GetProfitLossAsync(token, request, tokenCt),
            sections: ["Revenue", "Expense"],
            allowedAccountTypes: [FinanceAccountType.Revenue, FinanceAccountType.Expense],
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    [HttpGet("reports/cash-flow/export")]
    public Task<IActionResult> ExportCashFlow(
        string? search = null,
        string? sortBy = "section",
        string? sortDirection = "asc",
        int? periodId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? costCenterId = null,
        FinanceAccountType? accountType = null,
        string? section = null,
        CancellationToken ct = default)
    {
        return ExportFinancialStatementAsync(
            filePrefix: "cash-flow",
            fetcher: (token, request, tokenCt) => financeApiClient.GetCashFlowAsync(token, request, tokenCt),
            sections: ["Operating", "Investing", "Financing"],
            allowedAccountTypes:
            [
                FinanceAccountType.Asset,
                FinanceAccountType.Liability,
                FinanceAccountType.Equity,
                FinanceAccountType.Revenue,
                FinanceAccountType.Expense
            ],
            search: search,
            sortBy: sortBy,
            sortDirection: sortDirection,
            periodId: periodId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            costCenterId: costCenterId,
            accountType: accountType,
            section: section,
            ct: ct);
    }

    private async Task<IActionResult> RenderFinancialStatementAsync(
        string reportTitle,
        string reportKey,
        string exportActionName,
        Func<string, FinancialStatementPagedRequest, CancellationToken, Task<PagedResult<FinancialStatementRowDto>?>> fetcher,
        IReadOnlyList<string> sections,
        IReadOnlyList<FinanceAccountType> allowedAccountTypes,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string? sortDirection,
        int? periodId,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        int? costCenterId,
        FinanceAccountType? accountType,
        string? section,
        CancellationToken ct)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "section", "section", "accountcode", "accountname", "type", "amount");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);
        var normalizedSection = NormalizeSection(section, sections);
        var normalizedAccountType = NormalizeAccountType(accountType, allowedAccountTypes);

        var itemsTask = fetcher(accessToken, new FinancialStatementPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodId = periodId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            CostCenterId = costCenterId,
            AccountType = normalizedAccountType,
            Section = normalizedSection
        }, ct);

        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, periodOptionsTask, costCenterOptionsTask);

        ViewData["Title"] = reportTitle;
        ViewData["Breadcrumb"] = $"Finance / Financial Reports / {reportTitle}";

        return View("Reports/Statement", new FinanceStatementIndexViewModel
        {
            ReportTitle = reportTitle,
            ReportKey = reportKey,
            ExportActionName = exportActionName,
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodIdFilter = periodId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            CostCenterIdFilter = costCenterId,
            AccountTypeFilter = normalizedAccountType,
            SectionFilter = normalizedSection,
            AccountTypeOptions = allowedAccountTypes,
            SectionOptions = sections,
            PeriodOptions = await periodOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            Items = await itemsTask ?? PagedResult<FinancialStatementRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private async Task<IActionResult> ExportFinancialStatementAsync(
        string filePrefix,
        Func<string, FinancialStatementPagedRequest, CancellationToken, Task<PagedResult<FinancialStatementRowDto>?>> fetcher,
        IReadOnlyList<string> sections,
        IReadOnlyList<FinanceAccountType> allowedAccountTypes,
        string? search,
        string? sortBy,
        string? sortDirection,
        int? periodId,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        int? costCenterId,
        FinanceAccountType? accountType,
        string? section,
        CancellationToken ct)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedSortBy = NormalizeSortBy(sortBy, "section", "section", "accountcode", "accountname", "type", "amount");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);
        var normalizedSection = NormalizeSection(section, sections);
        var normalizedAccountType = NormalizeAccountType(accountType, allowedAccountTypes);

        var rows = await LoadAllFinancialStatementRowsAsync(
            accessToken,
            fetcher,
            new FinancialStatementPagedRequest
            {
                Page = 1,
                PageSize = 500,
                Search = search,
                SortBy = normalizedSortBy,
                SortDirection = normalizedSortDirection,
                PeriodId = periodId,
                DateFrom = normalizedDateFrom,
                DateTo = normalizedDateTo,
                CostCenterId = costCenterId,
                AccountType = normalizedAccountType,
                Section = normalizedSection
            },
            ct);

        var csvRows = rows.Select(x => (IReadOnlyList<string>)
        [
            x.Section,
            x.AccountCode,
            x.AccountName,
            x.AccountType.ToString(),
            x.Amount.ToString("0.00", CultureInfo.InvariantCulture)
        ]);

        return BuildCsvFile(
            filePrefix,
            ["Section", "Account Code", "Account Name", "Type", "Amount"],
            csvRows);
    }

    private async Task<IReadOnlyList<TrialBalanceRowDto>> LoadAllTrialBalanceRowsAsync(
        string accessToken,
        TrialBalancePagedRequest request,
        CancellationToken ct)
    {
        var rows = new List<TrialBalanceRowDto>();
        var currentPage = request.Page <= 0 ? 1 : request.Page;

        while (true)
        {
            request.Page = currentPage;
            var result = await financeApiClient.GetTrialBalanceAsync(accessToken, request, ct);
            if (result is null || result.Items.Count == 0)
            {
                break;
            }

            rows.AddRange(result.Items);
            if (result.Page >= result.TotalPages)
            {
                break;
            }

            currentPage++;
        }

        return rows;
    }

    private async Task<IReadOnlyList<FinancialStatementRowDto>> LoadAllFinancialStatementRowsAsync(
        string accessToken,
        Func<string, FinancialStatementPagedRequest, CancellationToken, Task<PagedResult<FinancialStatementRowDto>?>> fetcher,
        FinancialStatementPagedRequest request,
        CancellationToken ct)
    {
        var rows = new List<FinancialStatementRowDto>();
        var currentPage = request.Page <= 0 ? 1 : request.Page;

        while (true)
        {
            request.Page = currentPage;
            var result = await fetcher(accessToken, request, ct);
            if (result is null || result.Items.Count == 0)
            {
                break;
            }

            rows.AddRange(result.Items);
            if (result.Page >= result.TotalPages)
            {
                break;
            }

            currentPage++;
        }

        return rows;
    }

    private static FinanceAccountType? NormalizeAccountType(FinanceAccountType? accountType, IReadOnlyList<FinanceAccountType> allowedTypes)
    {
        if (!accountType.HasValue)
        {
            return null;
        }

        return allowedTypes.Contains(accountType.Value) ? accountType : null;
    }

    private static string? NormalizeSection(string? section, IReadOnlyList<string> allowedSections)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return null;
        }

        var normalized = section.Trim();
        return allowedSections.Any(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase))
            ? normalized
            : null;
    }

    private static FileContentResult BuildCsvFile(string filePrefix, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', headers.Select(CsvEscape)));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(CsvEscape)));
        }

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var fileName = $"{filePrefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return new FileContentResult(encoding.GetBytes(builder.ToString()), "text/csv")
        {
            FileDownloadName = fileName
        };
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}

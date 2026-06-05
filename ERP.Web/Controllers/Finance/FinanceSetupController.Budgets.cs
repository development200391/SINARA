using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("budgets")]
    public async Task<IActionResult> Budgets(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "budgetno",
        string? sortDirection = "asc",
        string? budgetNo = null,
        string? name = null,
        int? fiscalYearId = null,
        int? periodId = null,
        int? costCenterId = null,
        int? accountId = null,
        bool? isActive = null,
        decimal? amountFrom = null,
        decimal? amountTo = null,
        decimal? actualFrom = null,
        decimal? actualTo = null,
        decimal? varianceFrom = null,
        decimal? varianceTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "budgetno", "budgetno", "name", "fiscalyear", "period", "costcenter", "account", "totalamount", "totalactualamount", "totalvarianceamount", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedBudgetNo = NormalizeText(budgetNo);
        var normalizedName = NormalizeText(name);
        var (normalizedAmountFrom, normalizedAmountTo) = NormalizeDecimalRange(amountFrom, amountTo);
        var (normalizedActualFrom, normalizedActualTo) = NormalizeDecimalRange(actualFrom, actualTo);
        var (normalizedVarianceFrom, normalizedVarianceTo) = NormalizeDecimalRange(varianceFrom, varianceTo);

        var itemsTask = financeApiClient.GetBudgetsAsync(accessToken, new BudgetPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            BudgetNo = normalizedBudgetNo,
            Name = normalizedName,
            FiscalYearId = fiscalYearId,
            PeriodId = periodId,
            CostCenterId = costCenterId,
            AccountId = accountId,
            IsActive = isActive,
            AmountFrom = normalizedAmountFrom,
            AmountTo = normalizedAmountTo,
            ActualFrom = normalizedActualFrom,
            ActualTo = normalizedActualTo,
            VarianceFrom = normalizedVarianceFrom,
            VarianceTo = normalizedVarianceTo
        }, ct);

        var fiscalYearOptionsTask = LoadFiscalYearOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, fiscalYearOptionsTask, periodOptionsTask, costCenterOptionsTask, accountOptionsTask);

        ViewData["Title"] = "Budgets";
        ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets";

        return View("Budgets/Index", new FinanceBudgetsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            BudgetNoFilter = normalizedBudgetNo,
            NameFilter = normalizedName,
            FiscalYearIdFilter = fiscalYearId,
            PeriodIdFilter = periodId,
            CostCenterIdFilter = costCenterId,
            AccountIdFilter = accountId,
            IsActiveFilter = isActive,
            AmountFromFilter = normalizedAmountFrom,
            AmountToFilter = normalizedAmountTo,
            ActualFromFilter = normalizedActualFrom,
            ActualToFilter = normalizedActualTo,
            VarianceFromFilter = normalizedVarianceFrom,
            VarianceToFilter = normalizedVarianceTo,
            FiscalYearOptions = await fiscalYearOptionsTask,
            PeriodOptions = await periodOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            AccountOptions = await accountOptionsTask,
            Items = await itemsTask ?? PagedResult<BudgetDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
    [HttpGet("budgets/create")]
    public async Task<IActionResult> CreateBudget(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceBudgetEditViewModel
        {
            BudgetNo = $"BUD-{DateTime.UtcNow:yyyy}-001"
        };

        await PopulateBudgetFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Budget";
        ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Create";

        return View("Budgets/Create", model);
    }

    [HttpPost("budgets/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBudget(FinanceBudgetEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeBudgetForm(model);
        await PopulateBudgetFormOptionsAsync(accessToken, model, ct);
        ValidateBudgetForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Budget";
            ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Create";
            return View("Budgets/Create", model);
        }

        var created = await financeApiClient.CreateBudgetAsync(accessToken, MapBudgetRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create budget." : created.ErrorMessage);
            ViewData["Title"] = "Create Budget";
            ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Create";
            return View("Budgets/Create", model);
        }

        TempData["SuccessMessage"] = "Budget created.";
        return RedirectToAction(nameof(Budgets));
    }

    [HttpGet("budgets/edit/{id:int}")]
    public async Task<IActionResult> EditBudget(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var budget = await financeApiClient.GetBudgetByIdAsync(accessToken, id, ct);
        if (budget is null)
        {
            return NotFound();
        }

        var model = new FinanceBudgetEditViewModel
        {
            Id = budget.Id,
            BudgetNo = budget.BudgetNo,
            Name = budget.Name,
            FiscalYearId = budget.FiscalYearId,
            PeriodId = budget.PeriodId,
            CostCenterId = budget.CostCenterId,
            AccountId = budget.AccountId,
            CurrencyCode = budget.CurrencyCode,
            Notes = budget.Notes,
            IsActive = budget.IsActive,
            Lines = budget.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new FinanceBudgetLineEditViewModel
                {
                    Id = x.Id,
                    PeriodId = x.PeriodId,
                    AccountId = x.AccountId,
                    CostCenterId = x.CostCenterId,
                    Description = x.Description,
                    Amount = x.Amount
                })
                .ToList()
        };

        await PopulateBudgetFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Budget";
        ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Edit";

        return View("Budgets/Edit", model);
    }

    [HttpPost("budgets/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBudget(int id, FinanceBudgetEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeBudgetForm(model);
        await PopulateBudgetFormOptionsAsync(accessToken, model, ct);
        ValidateBudgetForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Budget";
            ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Edit";
            return View("Budgets/Edit", model);
        }

        var updated = await financeApiClient.UpdateBudgetAsync(accessToken, id, MapBudgetRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update budget." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Budget";
            ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budgets / Edit";
            return View("Budgets/Edit", model);
        }

        TempData["SuccessMessage"] = "Budget updated.";
        return RedirectToAction(nameof(Budgets));
    }

    [HttpPost("budgets/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBudget(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteBudgetAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Budget deleted." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to delete budget." : ok.ErrorMessage);
        return RedirectToAction(nameof(Budgets));
    }

    [HttpGet("reports/budget-vs-actual")]
    public async Task<IActionResult> BudgetVsActual(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "budgetno",
        string? sortDirection = "asc",
        int? budgetId = null,
        int? fiscalYearId = null,
        int? periodId = null,
        int? costCenterId = null,
        int? accountId = null,
        decimal? budgetFrom = null,
        decimal? budgetTo = null,
        decimal? actualFrom = null,
        decimal? actualTo = null,
        decimal? varianceFrom = null,
        decimal? varianceTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "budgetno", "budgetno", "budgetname", "fiscalyear", "period", "costcenter", "account", "budgetamount", "actualamount", "varianceamount", "utilizationpercentage");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedBudgetFrom, normalizedBudgetTo) = NormalizeDecimalRange(budgetFrom, budgetTo);
        var (normalizedActualFrom, normalizedActualTo) = NormalizeDecimalRange(actualFrom, actualTo);
        var (normalizedVarianceFrom, normalizedVarianceTo) = NormalizeDecimalRange(varianceFrom, varianceTo);

        var itemsTask = financeApiClient.GetBudgetVsActualAsync(accessToken, new BudgetVsActualPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            BudgetId = budgetId,
            FiscalYearId = fiscalYearId,
            PeriodId = periodId,
            CostCenterId = costCenterId,
            AccountId = accountId,
            BudgetFrom = normalizedBudgetFrom,
            BudgetTo = normalizedBudgetTo,
            ActualFrom = normalizedActualFrom,
            ActualTo = normalizedActualTo,
            VarianceFrom = normalizedVarianceFrom,
            VarianceTo = normalizedVarianceTo
        }, ct);

        var budgetOptionsTask = LoadBudgetOptionsAsync(accessToken, ct);
        var fiscalYearOptionsTask = LoadFiscalYearOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, budgetOptionsTask, fiscalYearOptionsTask, periodOptionsTask, costCenterOptionsTask, accountOptionsTask);

        ViewData["Title"] = "Budget vs Actual";
        ViewData["Breadcrumb"] = "Finance / Budget & Cost Control / Budget vs Actual";

        return View("Reports/BudgetVsActual", new FinanceBudgetVsActualIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            BudgetIdFilter = budgetId,
            FiscalYearIdFilter = fiscalYearId,
            PeriodIdFilter = periodId,
            CostCenterIdFilter = costCenterId,
            AccountIdFilter = accountId,
            BudgetFromFilter = normalizedBudgetFrom,
            BudgetToFilter = normalizedBudgetTo,
            ActualFromFilter = normalizedActualFrom,
            ActualToFilter = normalizedActualTo,
            VarianceFromFilter = normalizedVarianceFrom,
            VarianceToFilter = normalizedVarianceTo,
            BudgetOptions = await budgetOptionsTask,
            FiscalYearOptions = await fiscalYearOptionsTask,
            PeriodOptions = await periodOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            AccountOptions = await accountOptionsTask,
            Items = await itemsTask ?? PagedResult<BudgetVsActualRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private async Task PopulateBudgetFormOptionsAsync(string accessToken, FinanceBudgetEditViewModel model, CancellationToken ct)
    {
        var fiscalYearOptionsTask = LoadFiscalYearOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);

        await Task.WhenAll(fiscalYearOptionsTask, periodOptionsTask, costCenterOptionsTask, accountOptionsTask, currencyOptionsTask);

        model.FiscalYearOptions = await fiscalYearOptionsTask;
        model.PeriodOptions = await periodOptionsTask;
        model.CostCenterOptions = await costCenterOptionsTask;
        model.AccountOptions = await accountOptionsTask;
        model.CurrencyOptions = await currencyOptionsTask;

        if (model.FiscalYearId <= 0 || model.FiscalYearOptions.All(x => x.Id != model.FiscalYearId))
        {
            model.FiscalYearId = model.FiscalYearOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (model.PeriodId.HasValue && model.PeriodOptions.All(x => x.Id != model.PeriodId.Value))
        {
            model.PeriodId = null;
        }

        if (model.CostCenterId.HasValue && model.CostCenterOptions.All(x => x.Id != model.CostCenterId.Value))
        {
            model.CostCenterId = null;
        }

        if (model.AccountId.HasValue && model.AccountOptions.All(x => x.Id != model.AccountId.Value))
        {
            model.AccountId = null;
        }

        if (string.IsNullOrWhiteSpace(model.CurrencyCode) || model.CurrencyOptions.All(x => !string.Equals(x.Code, model.CurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            model.CurrencyCode = model.CurrencyOptions.FirstOrDefault()?.Code ?? "IDR";
        }

        if (model.Lines.Count == 0)
        {
            model.Lines = [new FinanceBudgetLineEditViewModel()];
        }
    }

    private static void NormalizeBudgetForm(FinanceBudgetEditViewModel model)
    {
        model.BudgetNo = string.IsNullOrWhiteSpace(model.BudgetNo) ? string.Empty : model.BudgetNo.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);
        model.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode)
            ? "IDR"
            : model.CurrencyCode.Trim().ToUpperInvariant();

        model.PeriodId = model.PeriodId is > 0 ? model.PeriodId : null;
        model.CostCenterId = model.CostCenterId is > 0 ? model.CostCenterId : null;
        model.AccountId = model.AccountId is > 0 ? model.AccountId : null;

        var normalizedLines = model.Lines
            .Where(x =>
                x.PeriodId > 0 ||
                x.AccountId > 0 ||
                x.CostCenterId.HasValue ||
                !string.IsNullOrWhiteSpace(x.Description) ||
                x.Amount > 0)
            .Select(x => new FinanceBudgetLineEditViewModel
            {
                Id = x.Id,
                PeriodId = x.PeriodId,
                AccountId = x.AccountId,
                CostCenterId = x.CostCenterId is > 0 ? x.CostCenterId : null,
                Description = NormalizeText(x.Description),
                Amount = x.Amount
            })
            .ToList();

        model.Lines = normalizedLines;

        if (model.Lines.Count == 0)
        {
            model.Lines = [new FinanceBudgetLineEditViewModel()];
        }
    }

    private void ValidateBudgetForm(FinanceBudgetEditViewModel model)
    {
        if (model.FiscalYearId <= 0)
        {
            ModelState.AddModelError(nameof(model.FiscalYearId), "Fiscal year is required.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "At least one budget line is required.");
            return;
        }

        for (var index = 0; index < model.Lines.Count; index++)
        {
            var line = model.Lines[index];

            if (line.PeriodId <= 0)
            {
                ModelState.AddModelError($"Lines[{index}].PeriodId", $"Period is required at line {index + 1}.");
            }

            if (line.AccountId <= 0)
            {
                ModelState.AddModelError($"Lines[{index}].AccountId", $"Account is required at line {index + 1}.");
            }

            if (line.Amount < 0)
            {
                ModelState.AddModelError($"Lines[{index}].Amount", $"Amount cannot be negative at line {index + 1}.");
            }
        }
    }

    private static BudgetDto MapBudgetRequest(FinanceBudgetEditViewModel model)
    {
        return new BudgetDto
        {
            Id = model.Id ?? 0,
            BudgetNo = model.BudgetNo,
            Name = model.Name,
            FiscalYearId = model.FiscalYearId,
            PeriodId = model.PeriodId,
            CostCenterId = model.CostCenterId,
            AccountId = model.AccountId,
            CurrencyCode = model.CurrencyCode,
            Notes = model.Notes,
            IsActive = model.IsActive,
            Lines = model.Lines
                .Select((x, index) => new BudgetLineDto
                {
                    Id = x.Id ?? 0,
                    LineNo = index + 1,
                    PeriodId = x.PeriodId,
                    AccountId = x.AccountId,
                    CostCenterId = x.CostCenterId,
                    Description = x.Description,
                    Amount = decimal.Round(x.Amount, 4, MidpointRounding.AwayFromZero)
                })
                .ToList()
        };
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadBudgetOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetBudgetsAsync(accessToken, new BudgetPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "budgetno",
            SortDirection = "asc"
        }, ct);

        return result?.Items
            .OrderBy(x => x.BudgetNo)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.BudgetNo} - {x.Name}"
            })
            .ToList() ?? [];
    }
}


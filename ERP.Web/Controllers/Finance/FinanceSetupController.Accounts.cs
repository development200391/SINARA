using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("coa")]
    public async Task<IActionResult> Coa(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? groupId = null,
        FinanceAccountType? type = null,
        FinanceNormalBalance? normalBalance = null,
        bool? isHeader = null,
        int? parentAccountId = null,
        string? currencyCode = null,
        bool? isBankAccount = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "groupname", "type", "normalbalance", "isheader", "currencycode", "isbankaccount", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var normalizedCurrencyCode = NormalizeText(currencyCode)?.ToUpperInvariant();

        var itemsTask = financeApiClient.GetAccountsAsync(accessToken, new AccountPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            GroupId = groupId,
            Type = type,
            NormalBalance = normalBalance,
            IsHeader = isHeader,
            ParentAccountId = parentAccountId,
            CurrencyCode = normalizedCurrencyCode,
            IsBankAccount = isBankAccount,
            IsActive = isActive
        }, ct);

        var groupOptionsTask = LoadAccountGroupOptionsAsync(accessToken, ct);
        var parentAccountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, groupOptionsTask, parentAccountOptionsTask, currencyOptionsTask);

        ViewData["Title"] = "Chart of Accounts";
        ViewData["Breadcrumb"] = "Finance / Chart of Accounts";

        return View("Accounts/Index", new FinanceAccountsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            GroupIdFilter = groupId,
            TypeFilter = type,
            NormalBalanceFilter = normalBalance,
            IsHeaderFilter = isHeader,
            ParentAccountIdFilter = parentAccountId,
            CurrencyCodeFilter = normalizedCurrencyCode,
            IsBankAccountFilter = isBankAccount,
            IsActiveFilter = isActive,
            GroupOptions = await groupOptionsTask,
            ParentAccountOptions = await parentAccountOptionsTask,
            CurrencyOptions = await currencyOptionsTask,
            Items = await itemsTask ?? PagedResult<AccountDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("coa/create")]
    public async Task<IActionResult> CreateCoa(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceAccountEditViewModel();
        await PopulateAccountFormOptionsAsync(accessToken, model, null, ct);

        ViewData["Title"] = "Create Account";
        ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Create";

        return View("Accounts/Create", model);
    }

    [HttpPost("coa/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCoa(FinanceAccountEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateAccountFormOptionsAsync(accessToken, model, null, ct);
        NormalizeAccountForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Account";
            ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Create";
            return View("Accounts/Create", model);
        }

        var created = await financeApiClient.CreateAccountAsync(accessToken, MapAccountDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create account.");
            ViewData["Title"] = "Create Account";
            ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Create";
            return View("Accounts/Create", model);
        }

        TempData["SuccessMessage"] = "Account created.";
        return RedirectToAction(nameof(Coa));
    }

    [HttpGet("coa/edit/{id:int}")]
    public async Task<IActionResult> EditCoa(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var account = await financeApiClient.GetAccountByIdAsync(accessToken, id, ct);
        if (account is null)
        {
            return NotFound();
        }

        var model = new FinanceAccountEditViewModel
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            GroupId = account.GroupId,
            Type = account.Type,
            NormalBalance = account.NormalBalance,
            IsHeader = account.IsHeader,
            ParentAccountId = account.ParentAccountId,
            Description = account.Description,
            IsBankAccount = account.IsBankAccount,
            BankName = account.BankName,
            BankAccountNo = account.BankAccountNo,
            CurrencyCode = account.CurrencyCode,
            IsActive = account.IsActive
        };

        await PopulateAccountFormOptionsAsync(accessToken, model, id, ct);

        ViewData["Title"] = "Edit Account";
        ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Edit";

        return View("Accounts/Edit", model);
    }

    [HttpPost("coa/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCoa(int id, FinanceAccountEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateAccountFormOptionsAsync(accessToken, model, id, ct);
        NormalizeAccountForm(model);

        if (model.ParentAccountId == id)
        {
            ModelState.AddModelError(nameof(model.ParentAccountId), "Parent account is invalid.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Account";
            ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Edit";
            return View("Accounts/Edit", model);
        }

        var updated = await financeApiClient.UpdateAccountAsync(accessToken, id, MapAccountDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update account.");
            ViewData["Title"] = "Edit Account";
            ViewData["Breadcrumb"] = "Finance / Chart of Accounts / Edit";
            return View("Accounts/Edit", model);
        }

        TempData["SuccessMessage"] = "Account updated.";
        return RedirectToAction(nameof(Coa));
    }

    [HttpPost("coa/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCoa(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteAccountAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Account deleted." : "Failed to delete account.";

        return RedirectToAction(nameof(Coa));
    }
}

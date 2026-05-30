using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    private async Task PopulateAccountGroupFormOptionsAsync(string accessToken, FinanceAccountGroupEditViewModel model, int? currentId, CancellationToken ct)
    {
        var options = await LoadAccountGroupOptionsAsync(accessToken, ct);
        if (currentId.HasValue)
        {
            options = options.Where(x => x.Id != currentId.Value).ToList();
        }

        if (model.ParentGroupId.HasValue && options.All(x => x.Id != model.ParentGroupId.Value))
        {
            model.ParentGroupId = null;
        }

        model.ParentGroupOptions = options;
    }

    private async Task PopulateAccountFormOptionsAsync(string accessToken, FinanceAccountEditViewModel model, int? currentId, CancellationToken ct)
    {
        var groupOptionsTask = LoadAccountGroupOptionsAsync(accessToken, ct);
        var parentAccountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);

        await Task.WhenAll(groupOptionsTask, parentAccountOptionsTask, currencyOptionsTask);

        var groupOptions = await groupOptionsTask;
        var parentAccountOptions = await parentAccountOptionsTask;
        var currencyOptions = await currencyOptionsTask;

        if (currentId.HasValue)
        {
            parentAccountOptions = parentAccountOptions.Where(x => x.Id != currentId.Value).ToList();
        }

        if (groupOptions.All(x => x.Id != model.GroupId))
        {
            model.GroupId = groupOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (model.ParentAccountId.HasValue && parentAccountOptions.All(x => x.Id != model.ParentAccountId.Value))
        {
            model.ParentAccountId = null;
        }

        if (!string.IsNullOrWhiteSpace(model.CurrencyCode) && currencyOptions.All(x => !string.Equals(x.Code, model.CurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            model.CurrencyCode = currencyOptions.FirstOrDefault()?.Code ?? "IDR";
        }

        if (string.IsNullOrWhiteSpace(model.CurrencyCode) && currencyOptions.Count > 0)
        {
            model.CurrencyCode = currencyOptions[0].Code;
        }

        model.GroupOptions = groupOptions;
        model.ParentAccountOptions = parentAccountOptions;
        model.CurrencyOptions = currencyOptions;
    }

    private async Task PopulateCostCenterFormOptionsAsync(string accessToken, FinanceCostCenterEditViewModel model, CancellationToken ct)
    {
        var departmentOptionsTask = LoadDepartmentOptionsAsync(accessToken, ct);
        var managerOptionsTask = LoadManagerOptionsAsync(accessToken, ct);
        var budgetAccountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);

        await Task.WhenAll(departmentOptionsTask, managerOptionsTask, budgetAccountOptionsTask);

        var departmentOptions = await departmentOptionsTask;
        var managerOptions = await managerOptionsTask;
        var budgetAccountOptions = await budgetAccountOptionsTask;

        if (model.DepartmentId.HasValue && departmentOptions.All(x => x.Id != model.DepartmentId.Value))
        {
            model.DepartmentId = null;
        }

        if (model.ManagerId.HasValue && managerOptions.All(x => x.Id != model.ManagerId.Value))
        {
            model.ManagerId = null;
        }

        if (model.BudgetAccountId.HasValue && budgetAccountOptions.All(x => x.Id != model.BudgetAccountId.Value))
        {
            model.BudgetAccountId = null;
        }

        model.DepartmentOptions = departmentOptions;
        model.ManagerOptions = managerOptions;
        model.BudgetAccountOptions = budgetAccountOptions;
    }

    private async Task PopulateExchangeRateFormOptionsAsync(string accessToken, FinanceExchangeRateCreateViewModel model, CancellationToken ct)
    {
        var currencyOptions = await LoadCurrencyOptionsAsync(accessToken, ct);

        if (!string.IsNullOrWhiteSpace(model.FromCurrencyCode) && currencyOptions.All(x => !string.Equals(x.Code, model.FromCurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            model.FromCurrencyCode = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(model.ToCurrencyCode) && currencyOptions.All(x => !string.Equals(x.Code, model.ToCurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            model.ToCurrencyCode = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(model.FromCurrencyCode) && currencyOptions.Count > 0)
        {
            model.FromCurrencyCode = currencyOptions[0].Code;
        }

        if (string.IsNullOrWhiteSpace(model.ToCurrencyCode) && currencyOptions.Count > 1)
        {
            model.ToCurrencyCode = currencyOptions[1].Code;
        }

        model.CurrencyOptions = currencyOptions;
    }

    private async Task PopulateTaxCodeFormOptionsAsync(string accessToken, FinanceTaxCodeEditViewModel model, CancellationToken ct)
    {
        var accountOptions = await LoadAccountOptionsAsync(accessToken, ct);

        if (accountOptions.All(x => x.Id != model.AccountId))
        {
            model.AccountId = accountOptions.FirstOrDefault()?.Id ?? 0;
        }

        model.AccountOptions = accountOptions;
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadAccountGroupOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetAccountGroupsAsync(accessToken, new AccountGroupPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadAccountOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetAccountsAsync(accessToken, new AccountPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceCodeOptionViewModel>> LoadCurrencyOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetCurrenciesAsync(accessToken, new CurrencyPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc",        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceCodeOptionViewModel
            {
                Code = x.Code,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadFiscalYearOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetFiscalYearsAsync(accessToken, new FiscalYearPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "startdate",
            SortDirection = "desc"
        }, ct);

        return result?.Items
            .OrderByDescending(x => x.StartDate)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadDepartmentOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        return result
            .OrderBy(x => x.Name)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadManagerOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);

        return result
            .OrderBy(x => x.Name)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToList();
    }
}




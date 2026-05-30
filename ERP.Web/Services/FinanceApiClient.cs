using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;

namespace ERP.Web.Services;

public sealed class FinanceApiClient(HttpClient httpClient, ILogger<FinanceApiClient> logger) : IFinanceApiClient
{
    public Task<PagedResult<AccountGroupDto>?> GetAccountGroupsAsync(string accessToken, AccountGroupPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.Type.HasValue)
        {
            parameters.Add($"type={(int)request.Type.Value}");
        }

        if (request.NormalBalance.HasValue)
        {
            parameters.Add($"normalBalance={(int)request.NormalBalance.Value}");
        }

        if (request.ParentGroupId.HasValue)
        {
            parameters.Add($"parentGroupId={request.ParentGroupId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/account-groups?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<AccountGroupDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<AccountGroupDto?> GetAccountGroupByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<AccountGroupDto>(HttpMethod.Get, $"api/v1/finance/account-groups/{id}", accessToken, null, ct);

    public Task<AccountGroupDto?> CreateAccountGroupAsync(string accessToken, AccountGroupDto request, CancellationToken ct = default)
        => SendAsync<AccountGroupDto>(HttpMethod.Post, "api/v1/finance/account-groups", accessToken, request, ct);

    public Task<AccountGroupDto?> UpdateAccountGroupAsync(string accessToken, int id, AccountGroupDto request, CancellationToken ct = default)
        => SendAsync<AccountGroupDto>(HttpMethod.Put, $"api/v1/finance/account-groups/{id}", accessToken, request, ct);

    public async Task<bool> DeleteAccountGroupAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/account-groups/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<AccountDto>?> GetAccountsAsync(string accessToken, AccountPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.GroupId.HasValue)
        {
            parameters.Add($"groupId={request.GroupId.Value}");
        }

        if (request.Type.HasValue)
        {
            parameters.Add($"type={(int)request.Type.Value}");
        }

        if (request.NormalBalance.HasValue)
        {
            parameters.Add($"normalBalance={(int)request.NormalBalance.Value}");
        }

        if (request.IsHeader.HasValue)
        {
            parameters.Add($"isHeader={(request.IsHeader.Value ? "true" : "false")}");
        }

        if (request.ParentAccountId.HasValue)
        {
            parameters.Add($"parentAccountId={request.ParentAccountId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            parameters.Add($"currencyCode={Uri.EscapeDataString(request.CurrencyCode.Trim().ToUpperInvariant())}");
        }

        if (request.IsBankAccount.HasValue)
        {
            parameters.Add($"isBankAccount={(request.IsBankAccount.Value ? "true" : "false")}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/accounts?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<AccountDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<AccountDto?> GetAccountByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<AccountDto>(HttpMethod.Get, $"api/v1/finance/accounts/{id}", accessToken, null, ct);

    public Task<AccountDto?> CreateAccountAsync(string accessToken, AccountDto request, CancellationToken ct = default)
        => SendAsync<AccountDto>(HttpMethod.Post, "api/v1/finance/accounts", accessToken, request, ct);

    public Task<AccountDto?> UpdateAccountAsync(string accessToken, int id, AccountDto request, CancellationToken ct = default)
        => SendAsync<AccountDto>(HttpMethod.Put, $"api/v1/finance/accounts/{id}", accessToken, request, ct);

    public async Task<bool> DeleteAccountAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/accounts/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<CostCenterDto>?> GetCostCentersAsync(string accessToken, CostCenterPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.ManagerId.HasValue)
        {
            parameters.Add($"managerId={request.ManagerId.Value}");
        }

        if (request.BudgetAccountId.HasValue)
        {
            parameters.Add($"budgetAccountId={request.BudgetAccountId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/cost-centers?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<CostCenterDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<CostCenterDto?> GetCostCenterByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<CostCenterDto>(HttpMethod.Get, $"api/v1/finance/cost-centers/{id}", accessToken, null, ct);

    public Task<CostCenterDto?> CreateCostCenterAsync(string accessToken, CostCenterDto request, CancellationToken ct = default)
        => SendAsync<CostCenterDto>(HttpMethod.Post, "api/v1/finance/cost-centers", accessToken, request, ct);

    public Task<CostCenterDto?> UpdateCostCenterAsync(string accessToken, int id, CostCenterDto request, CancellationToken ct = default)
        => SendAsync<CostCenterDto>(HttpMethod.Put, $"api/v1/finance/cost-centers/{id}", accessToken, request, ct);

    public async Task<bool> DeleteCostCenterAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/cost-centers/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<CurrencyDto>?> GetCurrenciesAsync(string accessToken, CurrencyPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Symbol))
        {
            parameters.Add($"symbol={Uri.EscapeDataString(request.Symbol.Trim())}");
        }

        if (request.IsBaseCurrency.HasValue)
        {
            parameters.Add($"isBaseCurrency={(request.IsBaseCurrency.Value ? "true" : "false")}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/currencies?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<CurrencyDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<CurrencyDto?> GetCurrencyByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<CurrencyDto>(HttpMethod.Get, $"api/v1/finance/currencies/{id}", accessToken, null, ct);

    public Task<CurrencyDto?> CreateCurrencyAsync(string accessToken, CurrencyDto request, CancellationToken ct = default)
        => SendAsync<CurrencyDto>(HttpMethod.Post, "api/v1/finance/currencies", accessToken, request, ct);

    public Task<CurrencyDto?> UpdateCurrencyAsync(string accessToken, int id, CurrencyDto request, CancellationToken ct = default)
        => SendAsync<CurrencyDto>(HttpMethod.Put, $"api/v1/finance/currencies/{id}", accessToken, request, ct);

    public async Task<bool> DeleteCurrencyAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/currencies/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ExchangeRateDto>?> GetExchangeRatesAsync(string accessToken, ExchangeRatePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.FromCurrencyCode))
        {
            parameters.Add($"fromCurrencyCode={Uri.EscapeDataString(request.FromCurrencyCode.Trim().ToUpperInvariant())}");
        }

        if (!string.IsNullOrWhiteSpace(request.ToCurrencyCode))
        {
            parameters.Add($"toCurrencyCode={Uri.EscapeDataString(request.ToCurrencyCode.Trim().ToUpperInvariant())}");
        }

        if (request.EffectiveDateFrom.HasValue)
        {
            parameters.Add($"effectiveDateFrom={request.EffectiveDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.EffectiveDateTo.HasValue)
        {
            parameters.Add($"effectiveDateTo={request.EffectiveDateTo.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            parameters.Add($"source={Uri.EscapeDataString(request.Source.Trim())}");
        }

        var query = $"api/v1/finance/exchange-rates?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ExchangeRateDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ExchangeRateDto?> GetExchangeRateByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ExchangeRateDto>(HttpMethod.Get, $"api/v1/finance/exchange-rates/{id}", accessToken, null, ct);

    public Task<ExchangeRateDto?> CreateExchangeRateAsync(string accessToken, ExchangeRateDto request, CancellationToken ct = default)
        => SendAsync<ExchangeRateDto>(HttpMethod.Post, "api/v1/finance/exchange-rates", accessToken, request, ct);

    public Task<PagedResult<FiscalYearDto>?> GetFiscalYearsAsync(string accessToken, FiscalYearPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.StartDateFrom.HasValue)
        {
            parameters.Add($"startDateFrom={request.StartDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.StartDateTo.HasValue)
        {
            parameters.Add($"startDateTo={request.StartDateTo.Value:yyyy-MM-dd}");
        }

        if (request.EndDateFrom.HasValue)
        {
            parameters.Add($"endDateFrom={request.EndDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.EndDateTo.HasValue)
        {
            parameters.Add($"endDateTo={request.EndDateTo.Value:yyyy-MM-dd}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        var query = $"api/v1/finance/fiscal-years?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<FiscalYearDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<FiscalYearDto?> GetFiscalYearByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FiscalYearDto>(HttpMethod.Get, $"api/v1/finance/fiscal-years/{id}", accessToken, null, ct);

    public Task<FiscalYearDto?> CreateFiscalYearAsync(string accessToken, FiscalYearDto request, CancellationToken ct = default)
        => SendAsync<FiscalYearDto>(HttpMethod.Post, "api/v1/finance/fiscal-years", accessToken, request, ct);

    public Task<FiscalYearDto?> UpdateFiscalYearAsync(string accessToken, int id, FiscalYearDto request, CancellationToken ct = default)
        => SendAsync<FiscalYearDto>(HttpMethod.Put, $"api/v1/finance/fiscal-years/{id}", accessToken, request, ct);

    public async Task<bool> CloseFiscalYearAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/finance/fiscal-years/{id}/close", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<PeriodDto>?> GetPeriodsAsync(string accessToken, PeriodPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.FiscalYearId.HasValue)
        {
            parameters.Add($"fiscalYearId={request.FiscalYearId.Value}");
        }

        if (request.PeriodNumberFrom.HasValue)
        {
            parameters.Add($"periodNumberFrom={request.PeriodNumberFrom.Value}");
        }

        if (request.PeriodNumberTo.HasValue)
        {
            parameters.Add($"periodNumberTo={request.PeriodNumberTo.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (request.StartDateFrom.HasValue)
        {
            parameters.Add($"startDateFrom={request.StartDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.StartDateTo.HasValue)
        {
            parameters.Add($"startDateTo={request.StartDateTo.Value:yyyy-MM-dd}");
        }

        var query = $"api/v1/finance/periods?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<PeriodDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PeriodDto?> GetPeriodByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<PeriodDto>(HttpMethod.Get, $"api/v1/finance/periods/{id}", accessToken, null, ct);

    public async Task<bool> ClosePeriodAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/finance/periods/{id}/close", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<TaxCodeDto>?> GetTaxCodesAsync(string accessToken, TaxCodePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.Type.HasValue)
        {
            parameters.Add($"type={(int)request.Type.Value}");
        }

        if (request.RateFrom.HasValue)
        {
            parameters.Add($"rateFrom={request.RateFrom.Value}");
        }

        if (request.RateTo.HasValue)
        {
            parameters.Add($"rateTo={request.RateTo.Value}");
        }

        if (request.IsInclusive.HasValue)
        {
            parameters.Add($"isInclusive={(request.IsInclusive.Value ? "true" : "false")}");
        }

        if (request.AccountId.HasValue)
        {
            parameters.Add($"accountId={request.AccountId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/tax-codes?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<TaxCodeDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<TaxCodeDto?> GetTaxCodeByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<TaxCodeDto>(HttpMethod.Get, $"api/v1/finance/tax-codes/{id}", accessToken, null, ct);

    public Task<TaxCodeDto?> CreateTaxCodeAsync(string accessToken, TaxCodeDto request, CancellationToken ct = default)
        => SendAsync<TaxCodeDto>(HttpMethod.Post, "api/v1/finance/tax-codes", accessToken, request, ct);

    public Task<TaxCodeDto?> UpdateTaxCodeAsync(string accessToken, int id, TaxCodeDto request, CancellationToken ct = default)
        => SendAsync<TaxCodeDto>(HttpMethod.Put, $"api/v1/finance/tax-codes/{id}", accessToken, request, ct);

    public async Task<bool> DeleteTaxCodeAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/tax-codes/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<JournalEntryDto>?> GetJournalsAsync(string accessToken, JournalPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.JournalNo))
        {
            parameters.Add($"journalNo={Uri.EscapeDataString(request.JournalNo.Trim())}");
        }

        if (request.DateFrom.HasValue)
        {
            parameters.Add($"dateFrom={request.DateFrom.Value:yyyy-MM-dd}");
        }

        if (request.DateTo.HasValue)
        {
            parameters.Add($"dateTo={request.DateTo.Value:yyyy-MM-dd}");
        }

        if (request.Source.HasValue)
        {
            parameters.Add($"source={(int)request.Source.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (request.PeriodId.HasValue)
        {
            parameters.Add($"periodId={request.PeriodId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.SourceRefType))
        {
            parameters.Add($"sourceRefType={Uri.EscapeDataString(request.SourceRefType.Trim())}");
        }

        var query = $"api/v1/finance/journals?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<JournalEntryDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<JournalEntryDto?> GetJournalByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<JournalEntryDto>(HttpMethod.Get, $"api/v1/finance/journals/{id}", accessToken, null, ct);

    public Task<JournalEntryDto?> CreateJournalAsync(string accessToken, JournalEntryDto request, CancellationToken ct = default)
        => SendAsync<JournalEntryDto>(HttpMethod.Post, "api/v1/finance/journals", accessToken, request, ct);

    public Task<JournalEntryDto?> UpdateJournalAsync(string accessToken, int id, JournalEntryDto request, CancellationToken ct = default)
        => SendAsync<JournalEntryDto>(HttpMethod.Put, $"api/v1/finance/journals/{id}", accessToken, request, ct);

    public Task<JournalEntryDto?> PostJournalAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<JournalEntryDto>(HttpMethod.Put, $"api/v1/finance/journals/{id}/post", accessToken, null, ct);

    public Task<JournalEntryDto?> ReverseJournalAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<JournalEntryDto>(HttpMethod.Put, $"api/v1/finance/journals/{id}/reverse", accessToken, null, ct);

    public Task<PagedResult<LedgerEntryDto>?> GetLedgerAsync(string accessToken, LedgerPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.AccountId.HasValue)
        {
            parameters.Add($"accountId={request.AccountId.Value}");
        }

        if (request.PeriodId.HasValue)
        {
            parameters.Add($"periodId={request.PeriodId.Value}");
        }

        if (request.CostCenterId.HasValue)
        {
            parameters.Add($"costCenterId={request.CostCenterId.Value}");
        }

        if (request.DateFrom.HasValue)
        {
            parameters.Add($"dateFrom={request.DateFrom.Value:yyyy-MM-dd}");
        }

        if (request.DateTo.HasValue)
        {
            parameters.Add($"dateTo={request.DateTo.Value:yyyy-MM-dd}");
        }

        var query = $"api/v1/finance/ledger?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<LedgerEntryDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }
    public Task<PagedResult<VendorDto>?> GetVendorsAsync(string accessToken, VendorPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.TaxId))
        {
            parameters.Add($"taxId={Uri.EscapeDataString(request.TaxId.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.ContactPerson))
        {
            parameters.Add($"contactPerson={Uri.EscapeDataString(request.ContactPerson.Trim())}");
        }

        if (request.PaymentTermsFrom.HasValue)
        {
            parameters.Add($"paymentTermsFrom={request.PaymentTermsFrom.Value}");
        }

        if (request.PaymentTermsTo.HasValue)
        {
            parameters.Add($"paymentTermsTo={request.PaymentTermsTo.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/vendors?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<VendorDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<VendorDto?> GetVendorByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<VendorDto>(HttpMethod.Get, $"api/v1/finance/vendors/{id}", accessToken, null, ct);

    public Task<VendorDto?> CreateVendorAsync(string accessToken, VendorDto request, CancellationToken ct = default)
        => SendAsync<VendorDto>(HttpMethod.Post, "api/v1/finance/vendors", accessToken, request, ct);

    public Task<VendorDto?> UpdateVendorAsync(string accessToken, int id, VendorDto request, CancellationToken ct = default)
        => SendAsync<VendorDto>(HttpMethod.Put, $"api/v1/finance/vendors/{id}", accessToken, request, ct);

    public async Task<bool> DeleteVendorAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/vendors/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ApInvoiceDto>?> GetApInvoicesAsync(string accessToken, ApInvoicePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.InvoiceNo))
        {
            parameters.Add($"invoiceNo={Uri.EscapeDataString(request.InvoiceNo.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.VendorInvoiceNo))
        {
            parameters.Add($"vendorInvoiceNo={Uri.EscapeDataString(request.VendorInvoiceNo.Trim())}");
        }

        if (request.VendorId.HasValue)
        {
            parameters.Add($"vendorId={request.VendorId.Value}");
        }

        if (request.PeriodId.HasValue)
        {
            parameters.Add($"periodId={request.PeriodId.Value}");
        }

        if (request.InvoiceDateFrom.HasValue)
        {
            parameters.Add($"invoiceDateFrom={request.InvoiceDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.InvoiceDateTo.HasValue)
        {
            parameters.Add($"invoiceDateTo={request.InvoiceDateTo.Value:yyyy-MM-dd}");
        }

        if (request.DueDateFrom.HasValue)
        {
            parameters.Add($"dueDateFrom={request.DueDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.DueDateTo.HasValue)
        {
            parameters.Add($"dueDateTo={request.DueDateTo.Value:yyyy-MM-dd}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (request.OutstandingFrom.HasValue)
        {
            parameters.Add($"outstandingFrom={request.OutstandingFrom.Value}");
        }

        if (request.OutstandingTo.HasValue)
        {
            parameters.Add($"outstandingTo={request.OutstandingTo.Value}");
        }

        if (request.IsOverdue.HasValue)
        {
            parameters.Add($"isOverdue={(request.IsOverdue.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/ap/invoices?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ApInvoiceDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ApInvoiceDto?> GetApInvoiceByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ApInvoiceDto>(HttpMethod.Get, $"api/v1/finance/ap/invoices/{id}", accessToken, null, ct);

    public Task<ApInvoiceDto?> CreateApInvoiceAsync(string accessToken, ApInvoiceDto request, CancellationToken ct = default)
        => SendAsync<ApInvoiceDto>(HttpMethod.Post, "api/v1/finance/ap/invoices", accessToken, request, ct);

    public Task<ApInvoiceDto?> UpdateApInvoiceAsync(string accessToken, int id, ApInvoiceDto request, CancellationToken ct = default)
        => SendAsync<ApInvoiceDto>(HttpMethod.Put, $"api/v1/finance/ap/invoices/{id}", accessToken, request, ct);

    public Task<ApInvoiceDto?> ApproveApInvoiceAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ApInvoiceDto>(HttpMethod.Put, $"api/v1/finance/ap/invoices/{id}/approve", accessToken, null, ct);

    public async Task<bool> DeleteApInvoiceAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/ap/invoices/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ApPaymentDto>?> GetApPaymentsAsync(string accessToken, ApPaymentPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.PaymentNo))
        {
            parameters.Add($"paymentNo={Uri.EscapeDataString(request.PaymentNo.Trim())}");
        }

        if (request.VendorId.HasValue)
        {
            parameters.Add($"vendorId={request.VendorId.Value}");
        }

        if (request.PaymentDateFrom.HasValue)
        {
            parameters.Add($"paymentDateFrom={request.PaymentDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.PaymentDateTo.HasValue)
        {
            parameters.Add($"paymentDateTo={request.PaymentDateTo.Value:yyyy-MM-dd}");
        }

        if (request.PaymentMethod.HasValue)
        {
            parameters.Add($"paymentMethod={(int)request.PaymentMethod.Value}");
        }

        if (request.AmountFrom.HasValue)
        {
            parameters.Add($"amountFrom={request.AmountFrom.Value}");
        }

        if (request.AmountTo.HasValue)
        {
            parameters.Add($"amountTo={request.AmountTo.Value}");
        }

        var query = $"api/v1/finance/ap/payments?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ApPaymentDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ApPaymentDto?> GetApPaymentByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ApPaymentDto>(HttpMethod.Get, $"api/v1/finance/ap/payments/{id}", accessToken, null, ct);

    public Task<ApPaymentDto?> CreateApPaymentAsync(string accessToken, ApPaymentDto request, CancellationToken ct = default)
        => SendAsync<ApPaymentDto>(HttpMethod.Post, "api/v1/finance/ap/payments", accessToken, request, ct);

    public Task<PagedResult<ApAgingRowDto>?> GetApAgingAsync(string accessToken, ApAgingPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.VendorId.HasValue)
        {
            parameters.Add($"vendorId={request.VendorId.Value}");
        }

        if (request.AsOfDate.HasValue)
        {
            parameters.Add($"asOfDate={request.AsOfDate.Value:yyyy-MM-dd}");
        }

        if (request.OutstandingMin.HasValue)
        {
            parameters.Add($"outstandingMin={request.OutstandingMin.Value}");
        }

        if (request.OutstandingMax.HasValue)
        {
            parameters.Add($"outstandingMax={request.OutstandingMax.Value}");
        }

        var query = $"api/v1/finance/ap/aging?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ApAgingRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }
    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        var response = await SendRawAsync(method, uri, accessToken, body, ct);
        if (response is null || !response.IsSuccessStatusCode)
        {
            return default;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize Finance API response from {Uri}", uri);
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException(uri);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call Finance API endpoint {Uri}", uri);
            return null;
        }
    }
}



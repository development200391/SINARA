using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;

namespace ERP.Web.Services;

public sealed class FinanceApiClient(HttpClient httpClient, ILogger<FinanceApiClient> logger) : ApiClientBase(httpClient, logger, "Finance"), IFinanceApiClient
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

    public Task<PagedResult<CustomerDto>?> GetCustomersAsync(string accessToken, CustomerPagedRequest request, CancellationToken ct = default)
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

        if (request.CreditLimitFrom.HasValue)
        {
            parameters.Add($"creditLimitFrom={request.CreditLimitFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.CreditLimitTo.HasValue)
        {
            parameters.Add($"creditLimitTo={request.CreditLimitTo.Value.ToString(CultureInfo.InvariantCulture)}");
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

        var query = $"api/v1/finance/customers?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<CustomerDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<CustomerDto?> GetCustomerByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<CustomerDto>(HttpMethod.Get, $"api/v1/finance/customers/{id}", accessToken, null, ct);

    public Task<CustomerDto?> CreateCustomerAsync(string accessToken, CustomerDto request, CancellationToken ct = default)
        => SendAsync<CustomerDto>(HttpMethod.Post, "api/v1/finance/customers", accessToken, request, ct);

    public Task<CustomerDto?> UpdateCustomerAsync(string accessToken, int id, CustomerDto request, CancellationToken ct = default)
        => SendAsync<CustomerDto>(HttpMethod.Put, $"api/v1/finance/customers/{id}", accessToken, request, ct);

    public async Task<bool> DeleteCustomerAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/customers/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<PeriodClosingRowDto>?> GetPeriodClosingAsync(string accessToken, PeriodClosingPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.FiscalYearId.HasValue)
        {
            parameters.Add($"fiscalYearId={request.FiscalYearId.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (request.DraftJournalFrom.HasValue)
        {
            parameters.Add($"draftJournalFrom={request.DraftJournalFrom.Value}");
        }

        if (request.DraftJournalTo.HasValue)
        {
            parameters.Add($"draftJournalTo={request.DraftJournalTo.Value}");
        }

        if (request.PendingApFrom.HasValue)
        {
            parameters.Add($"pendingApFrom={request.PendingApFrom.Value}");
        }

        if (request.PendingApTo.HasValue)
        {
            parameters.Add($"pendingApTo={request.PendingApTo.Value}");
        }

        if (request.PendingArFrom.HasValue)
        {
            parameters.Add($"pendingArFrom={request.PendingArFrom.Value}");
        }

        if (request.PendingArTo.HasValue)
        {
            parameters.Add($"pendingArTo={request.PendingArTo.Value}");
        }

        if (request.NetIncomeLossFrom.HasValue)
        {
            parameters.Add($"netIncomeLossFrom={request.NetIncomeLossFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.NetIncomeLossTo.HasValue)
        {
            parameters.Add($"netIncomeLossTo={request.NetIncomeLossTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var query = $"api/v1/finance/finalization/period-closing?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<PeriodClosingRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PagedResult<SmokeTestRowDto>?> GetSmokeTestsAsync(string accessToken, SmokeTestPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            parameters.Add($"category={Uri.EscapeDataString(request.Category.Trim())}");
        }

        if (request.Passed.HasValue)
        {
            parameters.Add($"passed={(request.Passed.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/finalization/smoke-tests?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<SmokeTestRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PagedResult<BudgetDto>?> GetBudgetsAsync(string accessToken, BudgetPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.BudgetNo))
        {
            parameters.Add($"budgetNo={Uri.EscapeDataString(request.BudgetNo.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.FiscalYearId.HasValue)
        {
            parameters.Add($"fiscalYearId={request.FiscalYearId.Value}");
        }

        if (request.PeriodId.HasValue)
        {
            parameters.Add($"periodId={request.PeriodId.Value}");
        }

        if (request.CostCenterId.HasValue)
        {
            parameters.Add($"costCenterId={request.CostCenterId.Value}");
        }

        if (request.AccountId.HasValue)
        {
            parameters.Add($"accountId={request.AccountId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        if (request.AmountFrom.HasValue)
        {
            parameters.Add($"amountFrom={request.AmountFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.AmountTo.HasValue)
        {
            parameters.Add($"amountTo={request.AmountTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.ActualFrom.HasValue)
        {
            parameters.Add($"actualFrom={request.ActualFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.ActualTo.HasValue)
        {
            parameters.Add($"actualTo={request.ActualTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.VarianceFrom.HasValue)
        {
            parameters.Add($"varianceFrom={request.VarianceFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.VarianceTo.HasValue)
        {
            parameters.Add($"varianceTo={request.VarianceTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var query = $"api/v1/finance/budgets?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<BudgetDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<BudgetDto?> GetBudgetByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<BudgetDto>(HttpMethod.Get, $"api/v1/finance/budgets/{id}", accessToken, null, ct);

    public Task<BudgetDto?> CreateBudgetAsync(string accessToken, BudgetDto request, CancellationToken ct = default)
        => SendAsync<BudgetDto>(HttpMethod.Post, "api/v1/finance/budgets", accessToken, request, ct);

    public Task<BudgetDto?> UpdateBudgetAsync(string accessToken, int id, BudgetDto request, CancellationToken ct = default)
        => SendAsync<BudgetDto>(HttpMethod.Put, $"api/v1/finance/budgets/{id}", accessToken, request, ct);

    public async Task<bool> DeleteBudgetAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/budgets/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ArInvoiceDto>?> GetArInvoicesAsync(string accessToken, ArInvoicePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.InvoiceNo))
        {
            parameters.Add($"invoiceNo={Uri.EscapeDataString(request.InvoiceNo.Trim())}");
        }

        if (request.CustomerId.HasValue)
        {
            parameters.Add($"customerId={request.CustomerId.Value}");
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
            parameters.Add($"outstandingFrom={request.OutstandingFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.OutstandingTo.HasValue)
        {
            parameters.Add($"outstandingTo={request.OutstandingTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.IsOverdue.HasValue)
        {
            parameters.Add($"isOverdue={(request.IsOverdue.Value ? "true" : "false")}");
        }

        var query = $"api/v1/finance/ar/invoices?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ArInvoiceDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ArInvoiceDto?> GetArInvoiceByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ArInvoiceDto>(HttpMethod.Get, $"api/v1/finance/ar/invoices/{id}", accessToken, null, ct);

    public Task<ArInvoiceDto?> CreateArInvoiceAsync(string accessToken, ArInvoiceDto request, CancellationToken ct = default)
        => SendAsync<ArInvoiceDto>(HttpMethod.Post, "api/v1/finance/ar/invoices", accessToken, request, ct);

    public Task<ArInvoiceDto?> UpdateArInvoiceAsync(string accessToken, int id, ArInvoiceDto request, CancellationToken ct = default)
        => SendAsync<ArInvoiceDto>(HttpMethod.Put, $"api/v1/finance/ar/invoices/{id}", accessToken, request, ct);

    public Task<ArInvoiceDto?> SendArInvoiceAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ArInvoiceDto>(HttpMethod.Put, $"api/v1/finance/ar/invoices/{id}/send", accessToken, null, ct);

    public async Task<bool> DeleteArInvoiceAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/finance/ar/invoices/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ArReceiptDto>?> GetArReceiptsAsync(string accessToken, ArReceiptPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.ReceiptNo))
        {
            parameters.Add($"receiptNo={Uri.EscapeDataString(request.ReceiptNo.Trim())}");
        }

        if (request.CustomerId.HasValue)
        {
            parameters.Add($"customerId={request.CustomerId.Value}");
        }

        if (request.ReceiptDateFrom.HasValue)
        {
            parameters.Add($"receiptDateFrom={request.ReceiptDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.ReceiptDateTo.HasValue)
        {
            parameters.Add($"receiptDateTo={request.ReceiptDateTo.Value:yyyy-MM-dd}");
        }

        if (request.PaymentMethod.HasValue)
        {
            parameters.Add($"paymentMethod={(int)request.PaymentMethod.Value}");
        }

        if (request.AmountFrom.HasValue)
        {
            parameters.Add($"amountFrom={request.AmountFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.AmountTo.HasValue)
        {
            parameters.Add($"amountTo={request.AmountTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var query = $"api/v1/finance/ar/receipts?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ArReceiptDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ArReceiptDto?> GetArReceiptByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ArReceiptDto>(HttpMethod.Get, $"api/v1/finance/ar/receipts/{id}", accessToken, null, ct);

    public Task<ArReceiptDto?> CreateArReceiptAsync(string accessToken, ArReceiptDto request, CancellationToken ct = default)
        => SendAsync<ArReceiptDto>(HttpMethod.Post, "api/v1/finance/ar/receipts", accessToken, request, ct);

    public Task<PagedResult<ArAgingRowDto>?> GetArAgingAsync(string accessToken, ArAgingPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.CustomerId.HasValue)
        {
            parameters.Add($"customerId={request.CustomerId.Value}");
        }

        if (request.AsOfDate.HasValue)
        {
            parameters.Add($"asOfDate={request.AsOfDate.Value:yyyy-MM-dd}");
        }

        if (request.OutstandingMin.HasValue)
        {
            parameters.Add($"outstandingMin={request.OutstandingMin.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.OutstandingMax.HasValue)
        {
            parameters.Add($"outstandingMax={request.OutstandingMax.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var query = $"api/v1/finance/ar/aging?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ArAgingRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }
    public Task<PagedResult<TrialBalanceRowDto>?> GetTrialBalanceAsync(string accessToken, TrialBalancePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddReportParameters(parameters, request.PeriodId, request.DateFrom, request.DateTo, request.AccountId, request.CostCenterId, request.Type, null);

        var query = $"api/v1/finance/reports/trial-balance?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<TrialBalanceRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PagedResult<FinancialStatementRowDto>?> GetBalanceSheetAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default)
        => GetFinancialStatementAsync(accessToken, "balance-sheet", request, ct);

    public Task<PagedResult<FinancialStatementRowDto>?> GetProfitLossAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default)
        => GetFinancialStatementAsync(accessToken, "profit-loss", request, ct);

    public Task<PagedResult<FinancialStatementRowDto>?> GetCashFlowAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default)
        => GetFinancialStatementAsync(accessToken, "cash-flow", request, ct);

    public Task<PagedResult<BudgetVsActualRowDto>?> GetBudgetVsActualAsync(string accessToken, BudgetVsActualPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.BudgetId.HasValue)
        {
            parameters.Add($"budgetId={request.BudgetId.Value}");
        }

        if (request.FiscalYearId.HasValue)
        {
            parameters.Add($"fiscalYearId={request.FiscalYearId.Value}");
        }

        if (request.PeriodId.HasValue)
        {
            parameters.Add($"periodId={request.PeriodId.Value}");
        }

        if (request.CostCenterId.HasValue)
        {
            parameters.Add($"costCenterId={request.CostCenterId.Value}");
        }

        if (request.AccountId.HasValue)
        {
            parameters.Add($"accountId={request.AccountId.Value}");
        }

        if (request.BudgetFrom.HasValue)
        {
            parameters.Add($"budgetFrom={request.BudgetFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.BudgetTo.HasValue)
        {
            parameters.Add($"budgetTo={request.BudgetTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.ActualFrom.HasValue)
        {
            parameters.Add($"actualFrom={request.ActualFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.ActualTo.HasValue)
        {
            parameters.Add($"actualTo={request.ActualTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.VarianceFrom.HasValue)
        {
            parameters.Add($"varianceFrom={request.VarianceFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.VarianceTo.HasValue)
        {
            parameters.Add($"varianceTo={request.VarianceTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var query = $"api/v1/finance/reports/budget-vs-actual?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<BudgetVsActualRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    private Task<PagedResult<FinancialStatementRowDto>?> GetFinancialStatementAsync(
        string accessToken,
        string reportPath,
        FinancialStatementPagedRequest request,
        CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddReportParameters(parameters, request.PeriodId, request.DateFrom, request.DateTo, null, request.CostCenterId, request.AccountType, request.Section);

        var query = $"api/v1/finance/reports/{reportPath}?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<FinancialStatementRowDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }
    private static void AddReportParameters(
        List<string> parameters,
        int? periodId,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        int? accountId,
        int? costCenterId,
        FinanceAccountType? accountType,
        string? section)
    {
        if (periodId.HasValue)
        {
            parameters.Add($"periodId={periodId.Value}");
        }

        if (dateFrom.HasValue)
        {
            parameters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            parameters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (accountId.HasValue)
        {
            parameters.Add($"accountId={accountId.Value}");
        }

        if (costCenterId.HasValue)
        {
            parameters.Add($"costCenterId={costCenterId.Value}");
        }

        if (accountType.HasValue)
        {
            parameters.Add($"accountType={(int)accountType.Value}");
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            parameters.Add($"section={Uri.EscapeDataString(section.Trim())}");
        }
    }
}


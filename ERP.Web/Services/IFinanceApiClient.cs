using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;

namespace ERP.Web.Services;

public interface IFinanceApiClient
{
    Task<PagedResult<AccountGroupDto>?> GetAccountGroupsAsync(string accessToken, AccountGroupPagedRequest request, CancellationToken ct = default);
    Task<AccountGroupDto?> GetAccountGroupByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<AccountGroupDto?> CreateAccountGroupAsync(string accessToken, AccountGroupDto request, CancellationToken ct = default);
    Task<AccountGroupDto?> UpdateAccountGroupAsync(string accessToken, int id, AccountGroupDto request, CancellationToken ct = default);
    Task<bool> DeleteAccountGroupAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<AccountDto>?> GetAccountsAsync(string accessToken, AccountPagedRequest request, CancellationToken ct = default);
    Task<AccountDto?> GetAccountByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<AccountDto?> CreateAccountAsync(string accessToken, AccountDto request, CancellationToken ct = default);
    Task<AccountDto?> UpdateAccountAsync(string accessToken, int id, AccountDto request, CancellationToken ct = default);
    Task<bool> DeleteAccountAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<CostCenterDto>?> GetCostCentersAsync(string accessToken, CostCenterPagedRequest request, CancellationToken ct = default);
    Task<CostCenterDto?> GetCostCenterByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<CostCenterDto?> CreateCostCenterAsync(string accessToken, CostCenterDto request, CancellationToken ct = default);
    Task<CostCenterDto?> UpdateCostCenterAsync(string accessToken, int id, CostCenterDto request, CancellationToken ct = default);
    Task<bool> DeleteCostCenterAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<CurrencyDto>?> GetCurrenciesAsync(string accessToken, CurrencyPagedRequest request, CancellationToken ct = default);
    Task<CurrencyDto?> GetCurrencyByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<CurrencyDto?> CreateCurrencyAsync(string accessToken, CurrencyDto request, CancellationToken ct = default);
    Task<CurrencyDto?> UpdateCurrencyAsync(string accessToken, int id, CurrencyDto request, CancellationToken ct = default);
    Task<bool> DeleteCurrencyAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ExchangeRateDto>?> GetExchangeRatesAsync(string accessToken, ExchangeRatePagedRequest request, CancellationToken ct = default);
    Task<ExchangeRateDto?> GetExchangeRateByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ExchangeRateDto?> CreateExchangeRateAsync(string accessToken, ExchangeRateDto request, CancellationToken ct = default);

    Task<PagedResult<FiscalYearDto>?> GetFiscalYearsAsync(string accessToken, FiscalYearPagedRequest request, CancellationToken ct = default);
    Task<FiscalYearDto?> GetFiscalYearByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FiscalYearDto?> CreateFiscalYearAsync(string accessToken, FiscalYearDto request, CancellationToken ct = default);
    Task<FiscalYearDto?> UpdateFiscalYearAsync(string accessToken, int id, FiscalYearDto request, CancellationToken ct = default);
    Task<bool> CloseFiscalYearAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PeriodDto>?> GetPeriodsAsync(string accessToken, PeriodPagedRequest request, CancellationToken ct = default);
    Task<PeriodDto?> GetPeriodByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ClosePeriodAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<TaxCodeDto>?> GetTaxCodesAsync(string accessToken, TaxCodePagedRequest request, CancellationToken ct = default);
    Task<TaxCodeDto?> GetTaxCodeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<TaxCodeDto?> CreateTaxCodeAsync(string accessToken, TaxCodeDto request, CancellationToken ct = default);
    Task<TaxCodeDto?> UpdateTaxCodeAsync(string accessToken, int id, TaxCodeDto request, CancellationToken ct = default);
    Task<bool> DeleteTaxCodeAsync(string accessToken, int id, CancellationToken ct = default);
}

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

    Task<PagedResult<JournalEntryDto>?> GetJournalsAsync(string accessToken, JournalPagedRequest request, CancellationToken ct = default);
    Task<JournalEntryDto?> GetJournalByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<JournalEntryDto?> CreateJournalAsync(string accessToken, JournalEntryDto request, CancellationToken ct = default);
    Task<JournalEntryDto?> UpdateJournalAsync(string accessToken, int id, JournalEntryDto request, CancellationToken ct = default);
    Task<JournalEntryDto?> PostJournalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<JournalEntryDto?> ReverseJournalAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LedgerEntryDto>?> GetLedgerAsync(string accessToken, LedgerPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<VendorDto>?> GetVendorsAsync(string accessToken, VendorPagedRequest request, CancellationToken ct = default);
    Task<VendorDto?> GetVendorByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<VendorDto?> CreateVendorAsync(string accessToken, VendorDto request, CancellationToken ct = default);
    Task<VendorDto?> UpdateVendorAsync(string accessToken, int id, VendorDto request, CancellationToken ct = default);
    Task<bool> DeleteVendorAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ApInvoiceDto>?> GetApInvoicesAsync(string accessToken, ApInvoicePagedRequest request, CancellationToken ct = default);
    Task<ApInvoiceDto?> GetApInvoiceByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApInvoiceDto?> CreateApInvoiceAsync(string accessToken, ApInvoiceDto request, CancellationToken ct = default);
    Task<ApInvoiceDto?> UpdateApInvoiceAsync(string accessToken, int id, ApInvoiceDto request, CancellationToken ct = default);
    Task<ApInvoiceDto?> ApproveApInvoiceAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> DeleteApInvoiceAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ApPaymentDto>?> GetApPaymentsAsync(string accessToken, ApPaymentPagedRequest request, CancellationToken ct = default);
    Task<ApPaymentDto?> GetApPaymentByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApPaymentDto?> CreateApPaymentAsync(string accessToken, ApPaymentDto request, CancellationToken ct = default);

    Task<PagedResult<ApAgingRowDto>?> GetApAgingAsync(string accessToken, ApAgingPagedRequest request, CancellationToken ct = default);

    Task<PagedResult<CustomerDto>?> GetCustomersAsync(string accessToken, CustomerPagedRequest request, CancellationToken ct = default);
    Task<CustomerDto?> GetCustomerByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<CustomerDto?> CreateCustomerAsync(string accessToken, CustomerDto request, CancellationToken ct = default);
    Task<CustomerDto?> UpdateCustomerAsync(string accessToken, int id, CustomerDto request, CancellationToken ct = default);
    Task<bool> DeleteCustomerAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ArInvoiceDto>?> GetArInvoicesAsync(string accessToken, ArInvoicePagedRequest request, CancellationToken ct = default);
    Task<ArInvoiceDto?> GetArInvoiceByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ArInvoiceDto?> CreateArInvoiceAsync(string accessToken, ArInvoiceDto request, CancellationToken ct = default);
    Task<ArInvoiceDto?> UpdateArInvoiceAsync(string accessToken, int id, ArInvoiceDto request, CancellationToken ct = default);
    Task<ArInvoiceDto?> SendArInvoiceAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> DeleteArInvoiceAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ArReceiptDto>?> GetArReceiptsAsync(string accessToken, ArReceiptPagedRequest request, CancellationToken ct = default);
    Task<ArReceiptDto?> GetArReceiptByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ArReceiptDto?> CreateArReceiptAsync(string accessToken, ArReceiptDto request, CancellationToken ct = default);

    Task<PagedResult<ArAgingRowDto>?> GetArAgingAsync(string accessToken, ArAgingPagedRequest request, CancellationToken ct = default);

    Task<PagedResult<TrialBalanceRowDto>?> GetTrialBalanceAsync(string accessToken, TrialBalancePagedRequest request, CancellationToken ct = default);
    Task<PagedResult<FinancialStatementRowDto>?> GetBalanceSheetAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<FinancialStatementRowDto>?> GetProfitLossAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<FinancialStatementRowDto>?> GetCashFlowAsync(string accessToken, FinancialStatementPagedRequest request, CancellationToken ct = default);
}





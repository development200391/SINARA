using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.Finance;

public sealed class AccountGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public FinanceAccountType Type { get; set; }
    public FinanceNormalBalance NormalBalance { get; set; }
    public int? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AccountGroupPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public FinanceAccountType? Type { get; set; }
    public FinanceNormalBalance? NormalBalance { get; set; }
    public int? ParentGroupId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class AccountDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public FinanceAccountType Type { get; set; }
    public FinanceNormalBalance NormalBalance { get; set; }
    public bool IsHeader { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? Description { get; set; }
    public bool IsBankAccount { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public bool IsActive { get; set; }
}

public sealed class AccountPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? GroupId { get; set; }
    public FinanceAccountType? Type { get; set; }
    public FinanceNormalBalance? NormalBalance { get; set; }
    public bool? IsHeader { get; set; }
    public int? ParentAccountId { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? IsBankAccount { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CostCenterDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int? BudgetAccountId { get; set; }
    public string? BudgetAccountName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CostCenterPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public int? BudgetAccountId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CurrencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CurrencyPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public bool? IsBaseCurrency { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ExchangeRateDto
{
    public int Id { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string? Source { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ExchangeRatePagedRequest : PagedRequest
{
    public string? FromCurrencyCode { get; set; }
    public string? ToCurrencyCode { get; set; }
    public DateOnly? EffectiveDateFrom { get; set; }
    public DateOnly? EffectiveDateTo { get; set; }
    public string? Source { get; set; }
}

public sealed class FiscalYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public FinancePeriodStatus Status { get; set; }
}

public sealed class FiscalYearPagedRequest : PagedRequest
{
    public string? Name { get; set; }
    public DateOnly? StartDateFrom { get; set; }
    public DateOnly? StartDateTo { get; set; }
    public DateOnly? EndDateFrom { get; set; }
    public DateOnly? EndDateTo { get; set; }
    public FinancePeriodStatus? Status { get; set; }
}

public sealed class PeriodDto
{
    public int Id { get; set; }
    public int FiscalYearId { get; set; }
    public string FiscalYearName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public FinancePeriodStatus Status { get; set; }
}

public sealed class PeriodPagedRequest : PagedRequest
{
    public int? FiscalYearId { get; set; }
    public int? PeriodNumberFrom { get; set; }
    public int? PeriodNumberTo { get; set; }
    public FinancePeriodStatus? Status { get; set; }
    public DateOnly? StartDateFrom { get; set; }
    public DateOnly? StartDateTo { get; set; }
}

public sealed class TaxCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FinanceTaxType Type { get; set; }
    public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class TaxCodePagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public FinanceTaxType? Type { get; set; }
    public decimal? RateFrom { get; set; }
    public decimal? RateTo { get; set; }
    public bool? IsInclusive { get; set; }
    public int? AccountId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class JournalLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitBase { get; set; }
    public decimal CreditBase { get; set; }
}

public sealed class JournalEntryDto
{
    public int Id { get; set; }
    public string JournalNo { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public FinanceJournalSource Source { get; set; }
    public int? SourceRefId { get; set; }
    public string? SourceRefType { get; set; }
    public FinanceJournalStatus Status { get; set; }
    public int? PostedBy { get; set; }
    public string? PostedByName { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public int? ReversedJournalId { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebitBase { get; set; }
    public decimal TotalCreditBase { get; set; }
    public IReadOnlyList<JournalLineDto> Lines { get; set; } = [];
}

public sealed class JournalPagedRequest : PagedRequest
{
    public string? JournalNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public FinanceJournalSource? Source { get; set; }
    public FinanceJournalStatus? Status { get; set; }
    public int? PeriodId { get; set; }
    public string? SourceRefType { get; set; }
}

public sealed class LedgerEntryDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string JournalNo { get; set; } = string.Empty;
    public string JournalDescription { get; set; } = string.Empty;
    public string? LineDescription { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public FinanceJournalSource Source { get; set; }
}

public sealed class LedgerPagedRequest : PagedRequest
{
    public int? AccountId { get; set; }
    public int? PeriodId { get; set; }
    public int? CostCenterId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public sealed class VendorDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public int? DefaultAccountId { get; set; }
    public string? DefaultAccountCode { get; set; }
    public string? DefaultAccountName { get; set; }
    public int? DefaultTaxCodeId { get; set; }
    public string? DefaultTaxCodeCode { get; set; }
    public string? DefaultTaxCodeName { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class VendorPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public string? ContactPerson { get; set; }
    public int? PaymentTermsFrom { get; set; }
    public int? PaymentTermsTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ApInvoiceLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int? TaxCodeId { get; set; }
    public string? TaxCodeCode { get; set; }
    public string? TaxCodeName { get; set; }
    public decimal TaxAmount { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
}

public sealed class ApInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string? VendorInvoiceNo { get; set; }
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;
    public FinanceApInvoiceStatus Status { get; set; } = FinanceApInvoiceStatus.Draft;
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsOverdue { get; set; }
    public IReadOnlyList<ApInvoiceLineDto> Lines { get; set; } = [];
}

public sealed class ApInvoicePagedRequest : PagedRequest
{
    public string? InvoiceNo { get; set; }
    public string? VendorInvoiceNo { get; set; }
    public int? VendorId { get; set; }
    public int? PeriodId { get; set; }
    public DateOnly? InvoiceDateFrom { get; set; }
    public DateOnly? InvoiceDateTo { get; set; }
    public DateOnly? DueDateFrom { get; set; }
    public DateOnly? DueDateTo { get; set; }
    public FinanceApInvoiceStatus? Status { get; set; }
    public decimal? OutstandingFrom { get; set; }
    public decimal? OutstandingTo { get; set; }
    public bool? IsOverdue { get; set; }
}

public sealed class ApPaymentApplicationDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal InvoiceTotalAmount { get; set; }
    public decimal InvoiceOutstandingAmount { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class ApPaymentDto
{
    public int Id { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public FinanceApPaymentMethod PaymentMethod { get; set; } = FinanceApPaymentMethod.Transfer;
    public int BankAccountId { get; set; }
    public string BankAccountCode { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int? JournalEntryId { get; set; }
    public IReadOnlyList<ApPaymentApplicationDto> Applications { get; set; } = [];
}

public sealed class ApPaymentPagedRequest : PagedRequest
{
    public string? PaymentNo { get; set; }
    public int? VendorId { get; set; }
    public DateOnly? PaymentDateFrom { get; set; }
    public DateOnly? PaymentDateTo { get; set; }
    public FinanceApPaymentMethod? PaymentMethod { get; set; }
    public decimal? AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
}

public sealed class ApAgingRowDto
{
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal Bucket1To30 { get; set; }
    public decimal Bucket31To60 { get; set; }
    public decimal Bucket61To90 { get; set; }
    public decimal BucketOver90 { get; set; }
    public decimal TotalOutstanding { get; set; }
    public DateOnly? OldestInvoiceDate { get; set; }
    public DateOnly? LatestDueDate { get; set; }
}

public sealed class ApAgingPagedRequest : PagedRequest
{
    public int? VendorId { get; set; }
    public DateOnly? AsOfDate { get; set; }
    public decimal? OutstandingMin { get; set; }
    public decimal? OutstandingMax { get; set; }
}

public sealed class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public int? DefaultAccountId { get; set; }
    public string? DefaultAccountCode { get; set; }
    public string? DefaultAccountName { get; set; }
    public int? DefaultTaxCodeId { get; set; }
    public string? DefaultTaxCodeCode { get; set; }
    public string? DefaultTaxCodeName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomerPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public string? ContactPerson { get; set; }
    public decimal? CreditLimitFrom { get; set; }
    public decimal? CreditLimitTo { get; set; }
    public int? PaymentTermsFrom { get; set; }
    public int? PaymentTermsTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ArInvoiceLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int? TaxCodeId { get; set; }
    public string? TaxCodeCode { get; set; }
    public string? TaxCodeName { get; set; }
    public decimal TaxAmount { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
}

public sealed class ArInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;
    public FinanceArInvoiceStatus Status { get; set; } = FinanceArInvoiceStatus.Draft;
    public int? SentBy { get; set; }
    public string? SentByName { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsOverdue { get; set; }
    public IReadOnlyList<ArInvoiceLineDto> Lines { get; set; } = [];
}

public sealed class ArInvoicePagedRequest : PagedRequest
{
    public string? InvoiceNo { get; set; }
    public int? CustomerId { get; set; }
    public int? PeriodId { get; set; }
    public DateOnly? InvoiceDateFrom { get; set; }
    public DateOnly? InvoiceDateTo { get; set; }
    public DateOnly? DueDateFrom { get; set; }
    public DateOnly? DueDateTo { get; set; }
    public FinanceArInvoiceStatus? Status { get; set; }
    public decimal? OutstandingFrom { get; set; }
    public decimal? OutstandingTo { get; set; }
    public bool? IsOverdue { get; set; }
}

public sealed class ArReceiptApplicationDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal InvoiceTotalAmount { get; set; }
    public decimal InvoiceOutstandingAmount { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class ArReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public FinanceArReceiptMethod PaymentMethod { get; set; } = FinanceArReceiptMethod.Transfer;
    public int BankAccountId { get; set; }
    public string BankAccountCode { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int? JournalEntryId { get; set; }
    public IReadOnlyList<ArReceiptApplicationDto> Applications { get; set; } = [];
}

public sealed class ArReceiptPagedRequest : PagedRequest
{
    public string? ReceiptNo { get; set; }
    public int? CustomerId { get; set; }
    public DateOnly? ReceiptDateFrom { get; set; }
    public DateOnly? ReceiptDateTo { get; set; }
    public FinanceArReceiptMethod? PaymentMethod { get; set; }
    public decimal? AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
}

public sealed class ArAgingRowDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal Bucket1To30 { get; set; }
    public decimal Bucket31To60 { get; set; }
    public decimal Bucket61To90 { get; set; }
    public decimal BucketOver90 { get; set; }
    public decimal TotalOutstanding { get; set; }
    public DateOnly? OldestInvoiceDate { get; set; }
    public DateOnly? LatestDueDate { get; set; }
}

public sealed class ArAgingPagedRequest : PagedRequest
{
    public int? CustomerId { get; set; }
    public DateOnly? AsOfDate { get; set; }
    public decimal? OutstandingMin { get; set; }
    public decimal? OutstandingMax { get; set; }
}

public sealed class TrialBalanceRowDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public FinanceAccountType AccountType { get; set; }
    public FinanceNormalBalance NormalBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
    public decimal EndingDebit { get; set; }
    public decimal EndingCredit { get; set; }
}

public sealed class TrialBalancePagedRequest : PagedRequest
{
    public int? PeriodId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public FinanceAccountType? Type { get; set; }
}

public sealed class FinancialStatementRowDto
{
    public string Section { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public FinanceAccountType AccountType { get; set; }
    public decimal Amount { get; set; }
}

public sealed class FinancialStatementPagedRequest : PagedRequest
{
    public int? PeriodId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? CostCenterId { get; set; }
    public FinanceAccountType? AccountType { get; set; }
    public string? Section { get; set; }
}

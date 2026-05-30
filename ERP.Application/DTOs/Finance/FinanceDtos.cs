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

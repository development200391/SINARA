using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Sales;

namespace ERP.Application.DTOs.Sales;

public sealed class SalesOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class SalesDashboardDto
{
    public int ActiveCustomerCategoryCount { get; set; }
    public int ActivePriceListCount { get; set; }
    public int ActiveSalesTeamCount { get; set; }
    public int OverCreditLimitCustomerCount { get; set; }
}

public sealed class SalesCustomerCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DefaultPriceListId { get; set; }
    public string? DefaultPriceListCode { get; set; }
    public string? DefaultPriceListName { get; set; }
    public int DefaultPaymentTerms { get; set; } = 2;
    public decimal DefaultCreditLimit { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SalesCustomerCategoryPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? DefaultPriceListId { get; set; }
    public int? DefaultPaymentTermsFrom { get; set; }
    public int? DefaultPaymentTermsTo { get; set; }
    public decimal? DefaultCreditLimitFrom { get; set; }
    public decimal? DefaultCreditLimitTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SalesPriceListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PriceListType Type { get; set; } = PriceListType.Standard;
    public string CurrencyCode { get; set; } = "IDR";
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class SalesPriceListPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public PriceListType? Type { get; set; }
    public string? CurrencyCode { get; set; }
    public DateOnly? ValidFromFrom { get; set; }
    public DateOnly? ValidFromTo { get; set; }
    public DateOnly? ValidToFrom { get; set; }
    public DateOnly? ValidToTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SalesPriceListItemDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int UomId { get; set; }
    public string UomCode { get; set; } = string.Empty;
    public string UomName { get; set; } = string.Empty;
    public decimal MinQty { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPct { get; set; }
}

public sealed class SalesPriceListItemPagedRequest : PagedRequest
{
    public int PriceListId { get; set; }
    public int? ItemId { get; set; }
    public int? UomId { get; set; }
    public decimal? MinQtyFrom { get; set; }
    public decimal? MinQtyTo { get; set; }
    public decimal? UnitPriceFrom { get; set; }
    public decimal? UnitPriceTo { get; set; }
    public decimal? DiscountPctFrom { get; set; }
    public decimal? DiscountPctTo { get; set; }
}

public sealed class SalesApprovalConfigDto
{
    public int Id { get; set; }
    public SalesDocumentType DocumentType { get; set; } = SalesDocumentType.SalesQuotation;
    public int Level { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? MaxDiscountPct { get; set; }
    public int? ApproverRoleId { get; set; }
    public string? ApproverRoleName { get; set; }
    public int? ApproverEmployeeId { get; set; }
    public string? ApproverEmployeeName { get; set; }
    public int TimeoutHours { get; set; } = 48;
    public bool AutoApproveIfTimeout { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SalesApprovalConfigPagedRequest : PagedRequest
{
    public SalesDocumentType? DocumentType { get; set; }
    public int? Level { get; set; }
    public decimal? MinAmountFrom { get; set; }
    public decimal? MinAmountTo { get; set; }
    public decimal? MaxAmountFrom { get; set; }
    public decimal? MaxAmountTo { get; set; }
    public decimal? MaxDiscountPctFrom { get; set; }
    public decimal? MaxDiscountPctTo { get; set; }
    public int? ApproverRoleId { get; set; }
    public int? ApproverEmployeeId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SalesTeamDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TeamLeaderId { get; set; }
    public string TeamLeaderName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int MemberCount { get; set; }
    public IReadOnlyList<int> MemberEmployeeIds { get; set; } = [];
    public IReadOnlyList<string> MemberEmployeeNames { get; set; } = [];
}

public sealed class SalesTeamPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? TeamLeaderId { get; set; }
    public int? MemberEmployeeId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SalesCustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? ContactPerson { get; set; }
    public int? CustomerCategoryId { get; set; }
    public string? CustomerCategoryCode { get; set; }
    public string? CustomerCategoryName { get; set; }
    public int? PriceListId { get; set; }
    public string? PriceListCode { get; set; }
    public string? PriceListName { get; set; }
    public int? SalesEmployeeId { get; set; }
    public string? SalesEmployeeName { get; set; }
    public int? SalesTeamId { get; set; }
    public string? SalesTeamCode { get; set; }
    public string? SalesTeamName { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CreditUsed { get; set; }
    public DateOnly? LastOrderDate { get; set; }
    public decimal TotalYtdSales { get; set; }
    public bool IsOverCreditLimit { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SalesCustomerPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? CustomerCategoryId { get; set; }
    public int? PriceListId { get; set; }
    public int? SalesEmployeeId { get; set; }
    public int? SalesTeamId { get; set; }
    public decimal? CreditLimitFrom { get; set; }
    public decimal? CreditLimitTo { get; set; }
    public decimal? CreditUsedFrom { get; set; }
    public decimal? CreditUsedTo { get; set; }
    public DateOnly? LastOrderDateFrom { get; set; }
    public DateOnly? LastOrderDateTo { get; set; }
    public decimal? TotalYtdSalesFrom { get; set; }
    public decimal? TotalYtdSalesTo { get; set; }
    public bool? IsOverCreditLimit { get; set; }
    public bool? IsActive { get; set; }
}

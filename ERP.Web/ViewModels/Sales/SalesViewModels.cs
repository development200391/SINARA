using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Enums.Sales;

namespace ERP.Web.ViewModels.Sales;

public sealed class SalesDashboardViewModel
{
    public SalesDashboardDto Data { get; set; } = new();
}

public sealed class SalesCustomerCategoriesIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? DefaultPriceListIdFilter { get; set; }
    public int? DefaultPaymentTermsFromFilter { get; set; }
    public int? DefaultPaymentTermsToFilter { get; set; }
    public decimal? DefaultCreditLimitFromFilter { get; set; }
    public decimal? DefaultCreditLimitToFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<SalesOptionDto> PriceListOptions { get; set; } = [];
    public PagedResult<SalesCustomerCategoryDto> Items { get; set; } = PagedResult<SalesCustomerCategoryDto>.Create([], 0, 1, 20);
}

public sealed class SalesCustomerCategoryEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? DefaultPriceListId { get; set; }

    [Range(0, int.MaxValue)]
    public int DefaultPaymentTerms { get; set; } = 2;

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal DefaultCreditLimit { get; set; }

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SalesOptionDto> PriceListOptions { get; set; } = [];
}

public sealed class SalesPriceListsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public PriceListType? TypeFilter { get; set; }
    public string? CurrencyCodeFilter { get; set; }
    public DateOnly? ValidFromFromFilter { get; set; }
    public DateOnly? ValidFromToFilter { get; set; }
    public DateOnly? ValidToFromFilter { get; set; }
    public DateOnly? ValidToToFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<string> CurrencyOptions { get; set; } = [];
    public PagedResult<SalesPriceListDto> Items { get; set; } = PagedResult<SalesPriceListDto>.Create([], 0, 1, 20);
}

public sealed class SalesPriceListEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public PriceListType Type { get; set; } = PriceListType.Standard;

    [Required]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "IDR";

    [Required]
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly? ValidTo { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public IReadOnlyList<string> CurrencyOptions { get; set; } = [];
}

public sealed class SalesPriceListDetailViewModel
{
    public SalesPriceListDto PriceList { get; set; } = new();

    public string? ItemSearch { get; set; }
    public int ItemPageSize { get; set; } = 20;
    public string ItemSortBy { get; set; } = "itemcode";
    public string ItemSortDirection { get; set; } = "asc";
    public int? ItemIdFilter { get; set; }
    public int? UomIdFilter { get; set; }
    public decimal? MinQtyFromFilter { get; set; }
    public decimal? MinQtyToFilter { get; set; }
    public decimal? UnitPriceFromFilter { get; set; }
    public decimal? UnitPriceToFilter { get; set; }
    public decimal? DiscountPctFromFilter { get; set; }
    public decimal? DiscountPctToFilter { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
    public PagedResult<SalesPriceListItemDto> Items { get; set; } = PagedResult<SalesPriceListItemDto>.Create([], 0, 1, 20);
}

public sealed class SalesPriceListItemEditViewModel
{
    public int? Id { get; set; }
    public int PriceListId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int UomId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999999.9999")]
    public decimal MinQty { get; set; } = 1m;

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal DiscountPct { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class SalesApprovalConfigsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "documenttype";
    public string SortDirection { get; set; } = "asc";

    public SalesDocumentType? DocumentTypeFilter { get; set; }
    public int? LevelFilter { get; set; }
    public decimal? MinAmountFromFilter { get; set; }
    public decimal? MinAmountToFilter { get; set; }
    public decimal? MaxAmountFromFilter { get; set; }
    public decimal? MaxAmountToFilter { get; set; }
    public decimal? MaxDiscountPctFromFilter { get; set; }
    public decimal? MaxDiscountPctToFilter { get; set; }
    public int? ApproverRoleIdFilter { get; set; }
    public int? ApproverEmployeeIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<RoleDto> RoleOptions { get; set; } = [];
    public IReadOnlyList<LookupDto> EmployeeOptions { get; set; } = [];
    public PagedResult<SalesApprovalConfigDto> Items { get; set; } = PagedResult<SalesApprovalConfigDto>.Create([], 0, 1, 20);
}

public sealed class SalesApprovalConfigEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public SalesDocumentType DocumentType { get; set; } = SalesDocumentType.SalesQuotation;

    [Range(1, int.MaxValue)]
    public int Level { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal MinAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal? MaxAmount { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? MaxDiscountPct { get; set; }

    public int? ApproverRoleId { get; set; }
    public int? ApproverEmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int TimeoutHours { get; set; } = 48;

    public bool AutoApproveIfTimeout { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<RoleDto> RoleOptions { get; set; } = [];
    public IReadOnlyList<LookupDto> EmployeeOptions { get; set; } = [];
}

public sealed class SalesTeamsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? TeamLeaderIdFilter { get; set; }
    public int? MemberEmployeeIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<LookupDto> EmployeeOptions { get; set; } = [];
    public PagedResult<SalesTeamDto> Items { get; set; } = PagedResult<SalesTeamDto>.Create([], 0, 1, 20);
}

public sealed class SalesTeamEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int TeamLeaderId { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> MemberEmployeeIds { get; set; } = [];
    public IReadOnlyList<LookupDto> EmployeeOptions { get; set; } = [];
}

public sealed class SalesCustomersIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? CustomerCategoryIdFilter { get; set; }
    public int? PriceListIdFilter { get; set; }
    public int? SalesEmployeeIdFilter { get; set; }
    public int? SalesTeamIdFilter { get; set; }
    public decimal? CreditLimitFromFilter { get; set; }
    public decimal? CreditLimitToFilter { get; set; }
    public decimal? CreditUsedFromFilter { get; set; }
    public decimal? CreditUsedToFilter { get; set; }
    public DateOnly? LastOrderDateFromFilter { get; set; }
    public DateOnly? LastOrderDateToFilter { get; set; }
    public decimal? TotalYtdSalesFromFilter { get; set; }
    public decimal? TotalYtdSalesToFilter { get; set; }
    public bool? IsOverCreditLimitFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<SalesOptionDto> CustomerCategoryOptions { get; set; } = [];
    public IReadOnlyList<SalesOptionDto> PriceListOptions { get; set; } = [];
    public IReadOnlyList<SalesOptionDto> SalesTeamOptions { get; set; } = [];
    public IReadOnlyList<LookupDto> SalesEmployeeOptions { get; set; } = [];
    public PagedResult<SalesCustomerDto> Items { get; set; } = PagedResult<SalesCustomerDto>.Create([], 0, 1, 20);
}

public sealed class SalesCustomerDetailViewModel
{
    public SalesCustomerDto Customer { get; set; } = new();
}




using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Purchasing;
using ERP.Domain.Enums.Purchasing;

namespace ERP.Web.ViewModels.Purchasing;

public sealed class PurchasingDashboardViewModel
{
    public PurchasingDashboardDto Data { get; init; } = new();
}

public sealed class PurchasingVendorCategoriesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<VendorCategoryDto> Items { get; init; } = PagedResult<VendorCategoryDto>.Create([], 0, 1, 20);
}

public sealed class PurchasingVendorCategoryEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class PurchasingApprovalConfigsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "documenttype";
    public string SortDirection { get; init; } = "asc";
    public PurchasingDocumentType? DocumentTypeFilter { get; init; }
    public int? LevelFilter { get; init; }
    public decimal? MinAmountFromFilter { get; init; }
    public decimal? MinAmountToFilter { get; init; }
    public decimal? MaxAmountFromFilter { get; init; }
    public decimal? MaxAmountToFilter { get; init; }
    public int? ApproverEmployeeIdFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<LookupDto> ApproverOptions { get; init; } = [];
    public PagedResult<ApprovalConfigDto> Items { get; init; } = PagedResult<ApprovalConfigDto>.Create([], 0, 1, 20);
}

public sealed class PurchasingApprovalConfigEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public PurchasingDocumentType DocumentType { get; set; } = PurchasingDocumentType.PurchaseRequisition;

    [Range(1, 99)]
    public int Level { get; set; } = 1;

    [Range(0, 9999999999999999d)]
    public decimal MinAmount { get; set; }

    [Range(0, 9999999999999999d)]
    public decimal? MaxAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int ApproverEmployeeId { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<LookupDto> ApproverOptions { get; set; } = [];
}

public sealed class PurchasingBuyerGroupsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? BuyerEmployeeIdFilter { get; init; }
    public int? ItemCategoryIdFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<LookupDto> BuyerOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemCategoryOptions { get; init; } = [];
    public PagedResult<BuyerGroupDto> Items { get; init; } = PagedResult<BuyerGroupDto>.Create([], 0, 1, 20);
}

public sealed class PurchasingBuyerGroupEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int BuyerEmployeeId { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ItemCategoryIds { get; set; } = [];

    public IReadOnlyList<LookupDto> BuyerOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemCategoryOptions { get; set; } = [];
}

public sealed class PurchasingVendorsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? VendorCategoryIdFilter { get; init; }
    public int? BuyerGroupIdFilter { get; init; }
    public bool? IsApprovedVendorFilter { get; init; }
    public decimal? PerformanceScoreFromFilter { get; init; }
    public decimal? PerformanceScoreToFilter { get; init; }
    public int? PaymentTermsFromFilter { get; init; }
    public int? PaymentTermsToFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<PurchasingOptionDto> VendorCategoryOptions { get; init; } = [];
    public IReadOnlyList<PurchasingOptionDto> BuyerGroupOptions { get; init; } = [];
    public PagedResult<PurchasingVendorDto> Items { get; init; } = PagedResult<PurchasingVendorDto>.Create([], 0, 1, 20);
}

public sealed class PurchasingVendorDetailViewModel
{
    public PurchasingVendorDto Vendor { get; init; } = new();
}

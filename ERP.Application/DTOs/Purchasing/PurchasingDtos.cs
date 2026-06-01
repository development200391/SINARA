using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Purchasing;

namespace ERP.Application.DTOs.Purchasing;

public sealed class PurchasingOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class VendorCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class VendorCategoryPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ApprovalConfigDto
{
    public int Id { get; set; }
    public PurchasingDocumentType DocumentType { get; set; } = PurchasingDocumentType.PurchaseRequisition;
    public int Level { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int ApproverEmployeeId { get; set; }
    public string ApproverEmployeeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class ApprovalConfigPagedRequest : PagedRequest
{
    public PurchasingDocumentType? DocumentType { get; set; }
    public int? Level { get; set; }
    public decimal? MinAmountFrom { get; set; }
    public decimal? MinAmountTo { get; set; }
    public decimal? MaxAmountFrom { get; set; }
    public decimal? MaxAmountTo { get; set; }
    public int? ApproverEmployeeId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class BuyerGroupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BuyerEmployeeId { get; set; }
    public string BuyerEmployeeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int MappedCategoryCount { get; set; }
    public string? ItemCategoryNames { get; set; }
    public IReadOnlyList<int> ItemCategoryIds { get; set; } = [];
}

public sealed class BuyerGroupPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? BuyerEmployeeId { get; set; }
    public int? ItemCategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class PurchasingVendorDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public int PaymentTermsDays { get; set; }
    public int? VendorCategoryId { get; set; }
    public string? VendorCategoryCode { get; set; }
    public string? VendorCategoryName { get; set; }
    public int? BuyerGroupId { get; set; }
    public string? BuyerGroupCode { get; set; }
    public string? BuyerGroupName { get; set; }
    public bool IsApprovedVendor { get; set; }
    public DateOnly? ApprovedDate { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? PerformanceScore { get; set; }
    public bool IsActive { get; set; }

    public int PoHistoryCount { get; set; }
    public decimal PoHistoryAmount { get; set; }
    public int ReturnCount { get; set; }
}

public sealed class PurchasingVendorPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? VendorCategoryId { get; set; }
    public int? BuyerGroupId { get; set; }
    public bool? IsApprovedVendor { get; set; }
    public decimal? PerformanceScoreFrom { get; set; }
    public decimal? PerformanceScoreTo { get; set; }
    public int? PaymentTermsFrom { get; set; }
    public int? PaymentTermsTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class PurchasingDashboardDto
{
    public int PendingPrApprovalCount { get; set; }
    public int OverduePoCount { get; set; }
    public decimal CurrentMonthPoAmount { get; set; }
    public int ApprovedVendorCount { get; set; }
}

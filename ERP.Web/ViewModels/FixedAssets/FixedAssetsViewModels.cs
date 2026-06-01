using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Web.ViewModels.FixedAssets;

public sealed class FixedAssetsDashboardViewModel
{
    public FixedAssetDashboardDto Data { get; init; } = new();
}

public sealed class FixedAssetCategoriesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public DepreciationMethod? DepreciationMethodFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<FixedAssetCategoryDto> Items { get; init; } = PagedResult<FixedAssetCategoryDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetCategoryEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;

    [Range(1, 1200)]
    public int UsefulLifeMonths { get; set; } = 60;

    [Range(typeof(decimal), "0", "100")]
    public decimal? DepreciationRate { get; set; }

    public int? AssetAccountId { get; set; }
    public int? AccumulatedDepreciationAccountId { get; set; }
    public int? DepreciationExpenseAccountId { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FixedAssetOptionDto> AccountOptions { get; set; } = [];
}

public sealed class FixedAssetLocationsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? DepartmentIdFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> DepartmentOptions { get; init; } = [];
    public PagedResult<FixedAssetLocationDto> Items { get; init; } = PagedResult<FixedAssetLocationDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetLocationEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FixedAssetOptionDto> DepartmentOptions { get; set; } = [];
    public IReadOnlyList<FixedAssetOptionDto> ManagerOptions { get; set; } = [];
}

public sealed class FixedAssetDepreciationConfigsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "fiscalyear";
    public string SortDirection { get; init; } = "desc";
    public short? FiscalYearFilter { get; init; }
    public bool? IsAutoPostJournalFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<FixedAssetDepreciationConfigDto> Items { get; init; } = PagedResult<FixedAssetDepreciationConfigDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetDepreciationConfigEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1900, 3000)]
    public short FiscalYear { get; set; } = (short)DateTime.UtcNow.Year;

    [Required]
    public DateOnly StartDate { get; set; } = new(DateTime.UtcNow.Year, 1, 1);

    [Required]
    public DateOnly EndDate { get; set; } = new(DateTime.UtcNow.Year, 12, 31);

    [Range(1, 31)]
    public byte RunDay { get; set; } = 28;

    public bool IsAutoPostJournal { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class FixedAssetsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "assetcode";
    public string SortDirection { get; init; } = "asc";
    public string? AssetCodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? LocationIdFilter { get; init; }
    public int? DepartmentIdFilter { get; init; }
    public AssetStatus? StatusFilter { get; init; }
    public decimal? BookValueFromFilter { get; init; }
    public decimal? BookValueToFilter { get; init; }
    public DateOnly? AcquisitionDateFromFilter { get; init; }
    public DateOnly? AcquisitionDateToFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> CategoryOptions { get; init; } = [];
    public IReadOnlyList<FixedAssetOptionDto> LocationOptions { get; init; } = [];
    public IReadOnlyList<FixedAssetOptionDto> DepartmentOptions { get; init; } = [];
    public PagedResult<FixedAssetDto> Items { get; init; } = PagedResult<FixedAssetDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetEditViewModel
{
    public int? Id { get; set; }

    [StringLength(30)]
    public string? AssetCode { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int LocationId { get; set; }

    public int? DepartmentId { get; set; }

    [Required]
    public DateOnly AcquisitionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public DateOnly InServiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal AcquisitionCost { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal SalvageValue { get; set; }

    [Range(1, 1200)]
    public int UsefulLifeMonths { get; set; } = 60;

    [Required]
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;

    [Range(typeof(decimal), "0", "100")]
    public decimal? DepreciationRate { get; set; }

    [Required]
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(150)]
    public string? VendorName { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FixedAssetOptionDto> CategoryOptions { get; set; } = [];
    public IReadOnlyList<FixedAssetOptionDto> LocationOptions { get; set; } = [];
    public IReadOnlyList<FixedAssetOptionDto> DepartmentOptions { get; set; } = [];
}

public sealed class FixedAssetDetailViewModel
{
    public FixedAssetDetailDto Data { get; init; } = new();
}

public sealed class FixedAssetDepreciationRunsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "periodyear";
    public string SortDirection { get; init; } = "desc";
    public short? PeriodYearFilter { get; init; }
    public byte? PeriodMonthFilter { get; init; }
    public DepreciationRunStatus? StatusFilter { get; init; }
    public PagedResult<FixedAssetDepreciationRunDto> Items { get; init; } = PagedResult<FixedAssetDepreciationRunDto>.Create([], 0, 1, 20);
    public FixedAssetRunDepreciationFormViewModel RunForm { get; init; } = new();
}

public sealed class FixedAssetRunDepreciationFormViewModel
{
    [Range(1900, 3000)]
    public short PeriodYear { get; set; } = (short)DateTime.UtcNow.Year;

    [Range(1, 12)]
    public byte PeriodMonth { get; set; } = (byte)DateTime.UtcNow.Month;

    public bool ApproveImmediately { get; set; }
}

public sealed class FixedAssetTransfersIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "transferdate";
    public string SortDirection { get; init; } = "desc";
    public int? AssetIdFilter { get; init; }
    public int? FromLocationIdFilter { get; init; }
    public int? ToLocationIdFilter { get; init; }
    public AssetTransferStatus? StatusFilter { get; init; }
    public DateOnly? TransferDateFromFilter { get; init; }
    public DateOnly? TransferDateToFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; init; } = [];
    public IReadOnlyList<FixedAssetOptionDto> LocationOptions { get; init; } = [];
    public PagedResult<FixedAssetTransferDto> Items { get; init; } = PagedResult<FixedAssetTransferDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetTransferEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int AssetId { get; set; }

    [Required]
    public DateOnly TransferDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(1, int.MaxValue)]
    public int FromLocationId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToLocationId { get; set; }

    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }

    [StringLength(2000)]
    public string? Reason { get; set; }

    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; set; } = [];
    public IReadOnlyList<FixedAssetOptionDto> LocationOptions { get; set; } = [];
    public IReadOnlyList<FixedAssetOptionDto> DepartmentOptions { get; set; } = [];
}

public sealed class FixedAssetMaintenanceOrdersIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "orderdate";
    public string SortDirection { get; init; } = "desc";
    public int? AssetIdFilter { get; init; }
    public MaintenanceType? MaintenanceTypeFilter { get; init; }
    public MaintenanceStatus? StatusFilter { get; init; }
    public DateOnly? OrderDateFromFilter { get; init; }
    public DateOnly? OrderDateToFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; init; } = [];
    public PagedResult<FixedAssetMaintenanceOrderDto> Items { get; init; } = PagedResult<FixedAssetMaintenanceOrderDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetMaintenanceOrderEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int AssetId { get; set; }

    [Required]
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.Preventive;

    [StringLength(150)]
    public string? VendorName { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Cost { get; set; }

    public bool IsCapitalized { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; set; } = [];
}

public sealed class FixedAssetDisposalsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "disposaldate";
    public string SortDirection { get; init; } = "desc";
    public int? AssetIdFilter { get; init; }
    public DisposalType? DisposalTypeFilter { get; init; }
    public DisposalStatus? StatusFilter { get; init; }
    public DateOnly? DisposalDateFromFilter { get; init; }
    public DateOnly? DisposalDateToFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; init; } = [];
    public PagedResult<FixedAssetDisposalDto> Items { get; init; } = PagedResult<FixedAssetDisposalDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetDisposalEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int AssetId { get; set; }

    [Required]
    public DateOnly DisposalDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public DisposalType DisposalType { get; set; } = DisposalType.Sale;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? SaleAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal DisposalExpense { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; set; } = [];
}

public sealed class FixedAssetRevaluationsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "revaluationdate";
    public string SortDirection { get; init; } = "desc";
    public int? AssetIdFilter { get; init; }
    public RevaluationStatus? StatusFilter { get; init; }
    public DateOnly? RevaluationDateFromFilter { get; init; }
    public DateOnly? RevaluationDateToFilter { get; init; }
    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; init; } = [];
    public PagedResult<FixedAssetRevaluationDto> Items { get; init; } = PagedResult<FixedAssetRevaluationDto>.Create([], 0, 1, 20);
}

public sealed class FixedAssetRevaluationEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int AssetId { get; set; }

    [Required]
    public DateOnly RevaluationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal OldBookValue { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal NewBookValue { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ImpairmentAmount { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public IReadOnlyList<FixedAssetOptionDto> AssetOptions { get; set; } = [];
}

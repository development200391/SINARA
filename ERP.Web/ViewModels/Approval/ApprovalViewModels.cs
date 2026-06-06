using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Approval;

namespace ERP.Web.ViewModels.Approval;

public sealed class ApprovalDashboardViewModel
{
    public ApprovalDashboardDto Data { get; init; } = new();
}

public sealed class ApprovalInboxIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "requestedat";
    public string SortDirection { get; init; } = "desc";
    public string? RequestNoFilter { get; init; }
    public string? ModuleFilter { get; init; }
    public string? ReferenceTypeFilter { get; init; }
    public ApprovalRequestStatus? StatusFilter { get; init; }
    public DateOnly? RequestedDateFromFilter { get; init; }
    public DateOnly? RequestedDateToFilter { get; init; }
    public bool? IsOverdueFilter { get; init; }
    public PagedResult<ApprovalInboxDto> Items { get; init; } = PagedResult<ApprovalInboxDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalMyRequestsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "requestedat";
    public string SortDirection { get; init; } = "desc";
    public string? RequestNoFilter { get; init; }
    public string? ModuleFilter { get; init; }
    public string? ReferenceTypeFilter { get; init; }
    public ApprovalRequestStatus? StatusFilter { get; init; }
    public DateOnly? RequestedDateFromFilter { get; init; }
    public DateOnly? RequestedDateToFilter { get; init; }
    public PagedResult<ApprovalRequestDto> Items { get; init; } = PagedResult<ApprovalRequestDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalTemplatesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public string? ModuleFilter { get; init; }
    public string? ReferenceTypeFilter { get; init; }
    public ApprovalType? ApprovalTypeFilter { get; init; }
    public decimal? MinAmountFromFilter { get; init; }
    public decimal? MinAmountToFilter { get; init; }
    public decimal? MaxAmountFromFilter { get; init; }
    public decimal? MaxAmountToFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<ApprovalTemplateDto> Items { get; init; } = PagedResult<ApprovalTemplateDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalTemplateEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ReferenceType { get; set; } = string.Empty;

    [Required]
    public ApprovalType ApprovalType { get; set; } = ApprovalType.Sequential;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? MinAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? MaxAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? AutoApproveBelow { get; set; }

    [Range(1, 720)]
    public int SlaHours { get; set; } = 24;

    public bool AllowDelegation { get; set; } = true;
    public bool RequireCommentOnReject { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class ApprovalLevelsIndexViewModel
{
    public int TemplateId { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public IReadOnlyList<ApprovalLevelDto> Items { get; init; } = [];
}

public sealed class ApprovalLevelEditViewModel
{
    public int? Id { get; set; }
    public int TemplateId { get; set; }

    [Range(1, 99)]
    public int LevelOrder { get; set; } = 1;

    [Required]
    [StringLength(100)]
    public string LevelName { get; set; } = string.Empty;

    [Required]
    public ApprovalApproverType ApproverType { get; set; } = ApprovalApproverType.Role;

    public int? ApproverRoleId { get; set; }
    public int? ApproverPositionId { get; set; }
    public int? ApproverUserId { get; set; }

    [Range(1, 99)]
    public int MinApproversRequired { get; set; } = 1;

    [Range(1, 720)]
    public int? EscalationHours { get; set; }

    public int? EscalateToLevelId { get; set; }
    public bool IsActive { get; set; } = true;

    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
}

public sealed class ApprovalDelegationsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "startdate";
    public string SortDirection { get; init; } = "desc";
    public int? DelegatorUserIdFilter { get; init; }
    public int? DelegateUserIdFilter { get; init; }
    public int? TemplateIdFilter { get; init; }
    public DateOnly? EffectiveDateFromFilter { get; init; }
    public DateOnly? EffectiveDateToFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<ApprovalOptionDto> UserOptions { get; init; } = [];
    public IReadOnlyList<ApprovalOptionDto> TemplateOptions { get; init; } = [];
    public PagedResult<ApprovalDelegationDto> Items { get; init; } = PagedResult<ApprovalDelegationDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalDelegationEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int DelegatorUserId { get; set; }

    [Range(1, int.MaxValue)]
    public int DelegateUserId { get; set; }

    public int? TemplateId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    [StringLength(2000)]
    public string? Reason { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ApprovalOptionDto> UserOptions { get; set; } = [];
    public IReadOnlyList<ApprovalOptionDto> TemplateOptions { get; set; } = [];
}

public sealed class ApprovalSlaReportIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "module";
    public string SortDirection { get; init; } = "asc";
    public string? ModuleFilter { get; init; }
    public int? TemplateIdFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public IReadOnlyList<ApprovalOptionDto> TemplateOptions { get; init; } = [];
    public PagedResult<ApprovalSlaReportDto> Items { get; init; } = PagedResult<ApprovalSlaReportDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalTemplateReportIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "templatecode";
    public string SortDirection { get; init; } = "asc";
    public string? ModuleFilter { get; init; }
    public int? TemplateIdFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public IReadOnlyList<ApprovalOptionDto> TemplateOptions { get; init; } = [];
    public PagedResult<ApprovalTemplateReportDto> Items { get; init; } = PagedResult<ApprovalTemplateReportDto>.Create([], 0, 1, 20);
}

public sealed class ApprovalAuditIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "createdat";
    public string SortDirection { get; init; } = "desc";
    public int? RequestIdFilter { get; init; }
    public int? ActorUserIdFilter { get; init; }
    public string? ActionFilter { get; init; }
    public string? ModuleFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public IReadOnlyList<ApprovalOptionDto> UserOptions { get; init; } = [];
    public PagedResult<ApprovalAuditLogDto> Items { get; init; } = PagedResult<ApprovalAuditLogDto>.Create([], 0, 1, 20);
}

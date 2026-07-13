using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Application.Services.Approval;

public interface IApprovalReportService
{
    Task<ApprovalDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);

    Task<PagedResult<ApprovalSlaReportDto>> GetSlaReportAsync(ApprovalSlaReportPagedRequest request, CancellationToken ct = default);

    Task<PagedResult<ApprovalTemplateReportDto>> GetTemplateReportAsync(ApprovalTemplateReportPagedRequest request, CancellationToken ct = default);

    Task<PagedResult<ApprovalAuditLogDto>> GetAuditLogsAsync(ApprovalAuditPagedRequest request, CancellationToken ct = default);
}

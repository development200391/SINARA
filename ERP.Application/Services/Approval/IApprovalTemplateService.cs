using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Application.Services.Approval;

public interface IApprovalTemplateService
{
    Task<PagedResult<ApprovalTemplateDto>> GetTemplatesPagedAsync(ApprovalTemplatePagedRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ApprovalOptionDto>> GetTemplateOptionsAsync(CancellationToken ct = default);

    Task<ApprovalTemplateDto?> GetTemplateByIdAsync(int id, CancellationToken ct = default);

    Task<ApprovalTemplateDto> CreateTemplateAsync(ApprovalTemplateDto request, CancellationToken ct = default);

    Task<ApprovalTemplateDto> UpdateTemplateAsync(int id, ApprovalTemplateDto request, CancellationToken ct = default);

    Task SetTemplateActiveAsync(int id, bool isActive, CancellationToken ct = default);

    Task DeleteTemplateAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ApprovalLevelDto>> GetLevelsAsync(int templateId, CancellationToken ct = default);

    Task<ApprovalLevelDto?> GetLevelByIdAsync(int templateId, int levelId, CancellationToken ct = default);

    Task<ApprovalLevelDto> CreateLevelAsync(int templateId, ApprovalLevelDto request, CancellationToken ct = default);

    Task<ApprovalLevelDto> UpdateLevelAsync(int templateId, int levelId, ApprovalLevelDto request, CancellationToken ct = default);

    Task DeleteLevelAsync(int templateId, int levelId, CancellationToken ct = default);
}

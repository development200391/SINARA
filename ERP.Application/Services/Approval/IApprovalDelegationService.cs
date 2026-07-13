using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Application.Services.Approval;

public interface IApprovalDelegationService
{
    Task<PagedResult<ApprovalDelegationDto>> GetDelegationsPagedAsync(ApprovalDelegationPagedRequest request, CancellationToken ct = default);

    Task<ApprovalDelegationDto?> GetDelegationByIdAsync(int id, CancellationToken ct = default);

    Task<ApprovalDelegationDto> CreateDelegationAsync(ApprovalDelegationDto request, CancellationToken ct = default);

    Task<ApprovalDelegationDto> UpdateDelegationAsync(int id, ApprovalDelegationDto request, CancellationToken ct = default);

    Task RevokeDelegationAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ApprovalOptionDto>> GetApproverOptionsAsync(CancellationToken ct = default);
}

namespace ERP.Domain.Interfaces.Purchasing;

public interface IPurchaseRequisitionRepository
{
    Task<PurchaseRequisitionReadModel?> GetByCodeAsync(string prNo, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseRequisitionReadModel>> GetPendingApprovalAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseRequisitionReadModel>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseRequisitionReadModel>> SearchAsync(string? search, CancellationToken ct = default);
}

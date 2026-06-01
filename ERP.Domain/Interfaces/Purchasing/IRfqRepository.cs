namespace ERP.Domain.Interfaces.Purchasing;

public interface IRfqRepository
{
    Task<RfqReadModel?> GetByCodeAsync(string rfqNo, CancellationToken ct = default);
    Task<RfqReadModel?> GetWithQuotationsAsync(int rfqId, CancellationToken ct = default);
    Task<IReadOnlyList<RfqReadModel>> GetAwardedAsync(CancellationToken ct = default);
}

namespace ERP.Domain.Interfaces.Sales;

public interface ISalesQuotationRepository
{
    Task<SalesQuotationReadModel?> GetByCodeAsync(string quotationNo, CancellationToken ct = default);
    Task<IReadOnlyList<SalesQuotationReadModel>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesQuotationReadModel>> GetExpiringAsync(DateOnly asOfDate, int horizonDays, CancellationToken ct = default);
    Task<IReadOnlyList<SalesQuotationReadModel>> SearchAsync(string? search, CancellationToken ct = default);
}

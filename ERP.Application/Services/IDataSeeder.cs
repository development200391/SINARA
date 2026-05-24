namespace ERP.Application.Services;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}

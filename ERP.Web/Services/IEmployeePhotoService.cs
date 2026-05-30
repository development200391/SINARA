using Microsoft.AspNetCore.Http;

namespace ERP.Web.Services;

public sealed record EmployeePhotoCropData(double? X, double? Y, double? Width, double? Height);

public interface IEmployeePhotoService
{
    Task<string> SavePhotoAsync(IFormFile file, EmployeePhotoCropData cropData, CancellationToken ct = default);
    Task DeletePhotoAsync(string? photoPath, CancellationToken ct = default);
}
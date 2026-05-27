using ERP.Application.DTOs.Config;

namespace ERP.Web.Services.Exports;

public interface IAuditLogExcelExportService
{
    Task WriteAsync(string outputPath, IAsyncEnumerable<AuditLogDto> rows, CancellationToken ct = default);
}

using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.ViewModels.Document;

public sealed class DocumentReferenceTypeConfigsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public PagedResult<DocumentReferenceTypeConfigDto> Configs { get; set; } = PagedResult<DocumentReferenceTypeConfigDto>.Create([], 0, 1, 20);
}

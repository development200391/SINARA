using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.ViewModels.Document;

public sealed class DocumentCategoriesIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public PagedResult<DocumentCategoryDto> Categories { get; set; } = PagedResult<DocumentCategoryDto>.Create([], 0, 1, 20);
}

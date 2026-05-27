using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class PagedGridViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PagedGridViewModel model)
    {
        return View(model);
    }
}

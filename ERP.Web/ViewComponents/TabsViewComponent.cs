using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class TabsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(TabsViewModel model)
    {
        return View(model);
    }
}

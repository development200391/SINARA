using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormDateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormDateViewModel model)
    {
        return View(model);
    }
}

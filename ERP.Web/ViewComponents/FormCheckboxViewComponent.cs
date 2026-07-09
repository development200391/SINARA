using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormCheckboxViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormCheckboxViewModel model)
    {
        return View(model);
    }
}

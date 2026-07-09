using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormEmailViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormEmailViewModel model)
    {
        return View(model);
    }
}

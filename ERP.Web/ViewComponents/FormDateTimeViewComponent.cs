using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormDateTimeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormDateTimeViewModel model)
    {
        return View(model);
    }
}

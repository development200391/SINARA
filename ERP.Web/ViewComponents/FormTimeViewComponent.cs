using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormTimeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormTimeViewModel model)
    {
        return View(model);
    }
}

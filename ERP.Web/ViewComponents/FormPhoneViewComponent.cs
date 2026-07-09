using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormPhoneViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormPhoneViewModel model)
    {
        return View(model);
    }
}

using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class FormIconPickerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FormIconPickerViewModel model)
    {
        return View(model);
    }
}

using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class TextInputViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(TextInputViewModel model)
    {
        return View(model);
    }
}

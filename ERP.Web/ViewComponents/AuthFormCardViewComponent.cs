using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class AuthFormCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AuthFormCardViewModel model)
    {
        return View(model);
    }
}


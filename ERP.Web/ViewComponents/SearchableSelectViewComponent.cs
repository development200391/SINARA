using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class SearchableSelectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(SearchableSelectViewModel model)
    {
        return View(model);
    }
}

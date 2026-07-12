using ERP.Web.ViewModels.Document;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class GeneralDocumentListViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GeneralDocumentListViewModel model)
    {
        return View(model);
    }
}

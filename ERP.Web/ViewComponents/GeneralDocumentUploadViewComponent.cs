using ERP.Web.ViewModels.Document;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class GeneralDocumentUploadViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GeneralDocumentUploadViewModel model)
    {
        return View(model);
    }
}

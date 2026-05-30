using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class EmployeePhotoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(HrEmployeeEditViewModel model)
    {
        return View(model);
    }
}
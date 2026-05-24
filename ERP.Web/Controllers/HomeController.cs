using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["Breadcrumb"] = "Dashboard";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}

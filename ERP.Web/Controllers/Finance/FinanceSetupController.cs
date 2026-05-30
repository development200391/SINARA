using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("finance")]
public sealed class FinanceSetupController : Controller
{
    [HttpGet("coa")]
    public IActionResult Coa() => Render("Chart of Accounts", "Finance / Chart of Accounts");

    [HttpGet("coa/groups")]
    public IActionResult CoaGroups() => Render("Account Groups", "Finance / Account Groups");

    [HttpGet("cost-centers")]
    public IActionResult CostCenters() => Render("Cost Centers", "Finance / Cost Centers");

    [HttpGet("currencies")]
    public IActionResult Currencies() => Render("Currencies", "Finance / Currencies");

    [HttpGet("exchange-rates")]
    public IActionResult ExchangeRates() => Render("Exchange Rates", "Finance / Exchange Rates");

    [HttpGet("fiscal-years")]
    public IActionResult FiscalYears() => Render("Fiscal Years", "Finance / Fiscal Years");

    [HttpGet("periods")]
    public IActionResult Periods() => Render("Accounting Periods", "Finance / Periods");

    [HttpGet("tax-codes")]
    public IActionResult TaxCodes() => Render("Tax Codes", "Finance / Tax Codes");

    private IActionResult Render(string title, string breadcrumb)
    {
        ViewData["Title"] = title;
        ViewData["Breadcrumb"] = breadcrumb;
        return View("ComingSoon", title);
    }
}

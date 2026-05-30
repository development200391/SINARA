using ERP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("finance")]
public sealed partial class FinanceSetupController : Controller
{
    private const int DefaultPageSize = 20;

    private readonly IFinanceApiClient financeApiClient;
    private readonly IHrApiClient hrApiClient;

    public FinanceSetupController(IFinanceApiClient financeApiClient, IHrApiClient hrApiClient)
    {
        this.financeApiClient = financeApiClient;
        this.hrApiClient = hrApiClient;
    }
}

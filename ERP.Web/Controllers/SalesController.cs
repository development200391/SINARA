using System.Globalization;
using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Application.DTOs.Finance;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Enums.Sales;
using ERP.Web.Services;
using ERP.Web.ViewModels.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("sales")]
public sealed partial class SalesController(
    ISalesApiClient salesApiClient,
    IHrApiClient hrApiClient,
    IInventoryApiClient inventoryApiClient,
    IFinanceApiClient financeApiClient,
    IConfigApiClient configApiClient) : Controller
{
    private const int DefaultPageSize = 20;
}

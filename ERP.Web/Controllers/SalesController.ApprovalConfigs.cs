namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("approval-configs")]
    public async Task<IActionResult> ApprovalConfigs(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "documenttype",
        string? sortDirection = "asc",
        SalesDocumentType? documentType = null,
        int? level = null,
        decimal? minAmountFrom = null,
        decimal? minAmountTo = null,
        decimal? maxAmountFrom = null,
        decimal? maxAmountTo = null,
        decimal? maxDiscountPctFrom = null,
        decimal? maxDiscountPctTo = null,
        int? approverRoleId = null,
        int? approverEmployeeId = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "documenttype", "documenttype", "level", "minamount", "maxamount", "maxdiscountpct", "approverrolename", "approveremployeename", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedMinAmountFrom, normalizedMinAmountTo) = NormalizeDecimalRange(minAmountFrom, minAmountTo);
        var (normalizedMaxAmountFrom, normalizedMaxAmountTo) = NormalizeDecimalRange(maxAmountFrom, maxAmountTo);
        var (normalizedMaxDiscountFrom, normalizedMaxDiscountTo) = NormalizeDecimalRange(maxDiscountPctFrom, maxDiscountPctTo);

        var roleOptionsTask = configApiClient.GetRolesAsync(accessToken, ct);
        var employeeOptionsTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var itemsTask = salesApiClient.GetApprovalConfigsAsync(accessToken, new SalesApprovalConfigPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DocumentType = documentType,
            Level = level,
            MinAmountFrom = normalizedMinAmountFrom,
            MinAmountTo = normalizedMinAmountTo,
            MaxAmountFrom = normalizedMaxAmountFrom,
            MaxAmountTo = normalizedMaxAmountTo,
            MaxDiscountPctFrom = normalizedMaxDiscountFrom,
            MaxDiscountPctTo = normalizedMaxDiscountTo,
            ApproverRoleId = approverRoleId,
            ApproverEmployeeId = approverEmployeeId,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(roleOptionsTask, employeeOptionsTask, itemsTask);

        ViewData["Title"] = "Approval Configs";
        ViewData["Breadcrumb"] = "Sales / Approval Configs";

        return View("ApprovalConfigs/Index", new SalesApprovalConfigsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DocumentTypeFilter = documentType,
            LevelFilter = level,
            MinAmountFromFilter = normalizedMinAmountFrom,
            MinAmountToFilter = normalizedMinAmountTo,
            MaxAmountFromFilter = normalizedMaxAmountFrom,
            MaxAmountToFilter = normalizedMaxAmountTo,
            MaxDiscountPctFromFilter = normalizedMaxDiscountFrom,
            MaxDiscountPctToFilter = normalizedMaxDiscountTo,
            ApproverRoleIdFilter = approverRoleId,
            ApproverEmployeeIdFilter = approverEmployeeId,
            IsActiveFilter = isActive,
            RoleOptions = (await roleOptionsTask).Where(x => x.IsActive).OrderBy(x => x.Name).ToList(),
            EmployeeOptions = (await employeeOptionsTask).OrderBy(x => x.Name).ToList(),
            Items = await itemsTask ?? PagedResult<SalesApprovalConfigDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("approval-configs/create")]
    public async Task<IActionResult> CreateApprovalConfig(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new SalesApprovalConfigEditViewModel
        {
            IsActive = true,
            TimeoutHours = 48,
            RoleOptions = (await configApiClient.GetRolesAsync(accessToken, ct)).Where(x => x.IsActive).OrderBy(x => x.Name).ToList(),
            EmployeeOptions = (await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct)).OrderBy(x => x.Name).ToList()
        };

        ViewData["Title"] = "Create Approval Config";
        ViewData["Breadcrumb"] = "Sales / Approval Configs / Create";

        return View("ApprovalConfigs/Create", model);
    }

    [HttpPost("approval-configs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApprovalConfig(SalesApprovalConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeApprovalConfigForm(model);
        await PopulateApprovalConfigFormOptionsAsync(accessToken, model, ct);
        ValidateApprovalConfigForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Approval Config";
            ViewData["Breadcrumb"] = "Sales / Approval Configs / Create";
            return View("ApprovalConfigs/Create", model);
        }

        var created = await salesApiClient.CreateApprovalConfigAsync(accessToken, MapApprovalConfigRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create approval config." : created.ErrorMessage);
            ViewData["Title"] = "Create Approval Config";
            ViewData["Breadcrumb"] = "Sales / Approval Configs / Create";
            return View("ApprovalConfigs/Create", model);
        }

        TempData["SuccessMessage"] = "Approval config created.";
        return RedirectToAction(nameof(ApprovalConfigs));
    }

    [HttpGet("approval-configs/edit/{id:int}")]
    public async Task<IActionResult> EditApprovalConfig(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await salesApiClient.GetApprovalConfigByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new SalesApprovalConfigEditViewModel
        {
            Id = item.Id,
            DocumentType = item.DocumentType,
            Level = item.Level,
            MinAmount = item.MinAmount,
            MaxAmount = item.MaxAmount,
            MaxDiscountPct = item.MaxDiscountPct,
            ApproverRoleId = item.ApproverRoleId,
            ApproverEmployeeId = item.ApproverEmployeeId,
            TimeoutHours = item.TimeoutHours,
            AutoApproveIfTimeout = item.AutoApproveIfTimeout,
            IsActive = item.IsActive
        };

        await PopulateApprovalConfigFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Approval Config";
        ViewData["Breadcrumb"] = "Sales / Approval Configs / Edit";

        return View("ApprovalConfigs/Edit", model);
    }

    [HttpPost("approval-configs/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditApprovalConfig(int id, SalesApprovalConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeApprovalConfigForm(model);
        await PopulateApprovalConfigFormOptionsAsync(accessToken, model, ct);
        ValidateApprovalConfigForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Approval Config";
            ViewData["Breadcrumb"] = "Sales / Approval Configs / Edit";
            return View("ApprovalConfigs/Edit", model);
        }

        var updated = await salesApiClient.UpdateApprovalConfigAsync(accessToken, id, MapApprovalConfigRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update approval config." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Approval Config";
            ViewData["Breadcrumb"] = "Sales / Approval Configs / Edit";
            return View("ApprovalConfigs/Edit", model);
        }

        TempData["SuccessMessage"] = "Approval config updated.";
        return RedirectToAction(nameof(ApprovalConfigs));
    }

    [HttpPost("approval-configs/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteApprovalConfig(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await salesApiClient.DeleteApprovalConfigAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Approval config deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete approval config." : deleted.ErrorMessage);

        return RedirectToAction(nameof(ApprovalConfigs));
    }
}

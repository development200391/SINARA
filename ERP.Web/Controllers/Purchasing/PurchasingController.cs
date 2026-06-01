using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Purchasing;
using ERP.Domain.Enums.Purchasing;
using ERP.Web.Services;
using ERP.Web.ViewModels.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("purchasing")]
public sealed class PurchasingController(
    IPurchasingApiClient purchasingApiClient,
    IHrApiClient hrApiClient,
    IInventoryApiClient inventoryApiClient) : Controller
{
    private const int DefaultPageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dashboard = await purchasingApiClient.GetDashboardAsync(accessToken, ct) ?? new PurchasingDashboardDto();

        ViewData["Title"] = "Purchasing Dashboard";
        ViewData["Breadcrumb"] = "Purchasing / Dashboard";

        return View("Index", new PurchasingDashboardViewModel
        {
            Data = dashboard
        });
    }

    [HttpGet("vendor-categories")]
    public async Task<IActionResult> VendorCategories(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await purchasingApiClient.GetVendorCategoriesAsync(accessToken, new VendorCategoryPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Vendor Categories";
        ViewData["Breadcrumb"] = "Purchasing / Vendor Categories";

        return View("VendorCategories/Index", new PurchasingVendorCategoriesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<VendorCategoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("vendor-categories/create")]
    public IActionResult CreateVendorCategory()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Vendor Category";
        ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Create";

        return View("VendorCategories/Create", new PurchasingVendorCategoryEditViewModel
        {
            IsActive = true
        });
    }

    [HttpPost("vendor-categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVendorCategory(PurchasingVendorCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Vendor Category";
            ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Create";
            return View("VendorCategories/Create", model);
        }

        var created = await purchasingApiClient.CreateVendorCategoryAsync(accessToken, MapVendorCategoryDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create vendor category.");
            ViewData["Title"] = "Create Vendor Category";
            ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Create";
            return View("VendorCategories/Create", model);
        }

        TempData["SuccessMessage"] = "Vendor category created.";
        return RedirectToAction(nameof(VendorCategories));
    }

    [HttpGet("vendor-categories/edit/{id:int}")]
    public async Task<IActionResult> EditVendorCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await purchasingApiClient.GetVendorCategoryByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Vendor Category";
        ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Edit";

        return View("VendorCategories/Edit", new PurchasingVendorCategoryEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            IsActive = item.IsActive
        });
    }

    [HttpPost("vendor-categories/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVendorCategory(int id, PurchasingVendorCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Vendor Category";
            ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Edit";
            return View("VendorCategories/Edit", model);
        }

        var updated = await purchasingApiClient.UpdateVendorCategoryAsync(accessToken, id, MapVendorCategoryDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update vendor category.");
            ViewData["Title"] = "Edit Vendor Category";
            ViewData["Breadcrumb"] = "Purchasing / Vendor Categories / Edit";
            return View("VendorCategories/Edit", model);
        }

        TempData["SuccessMessage"] = "Vendor category updated.";
        return RedirectToAction(nameof(VendorCategories));
    }

    [HttpPost("vendor-categories/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVendorCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await purchasingApiClient.DeleteVendorCategoryAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Vendor category deleted."
            : "Failed to delete vendor category.";

        return RedirectToAction(nameof(VendorCategories));
    }

    [HttpGet("approval-configs")]
    public async Task<IActionResult> ApprovalConfigs(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "documenttype",
        string? sortDirection = "asc",
        PurchasingDocumentType? documentType = null,
        int? level = null,
        decimal? minAmountFrom = null,
        decimal? minAmountTo = null,
        decimal? maxAmountFrom = null,
        decimal? maxAmountTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "documenttype", "documenttype", "level", "minamount", "maxamount", "approveremployeename", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var approverOptionsTask = GetApproverOptionsAsync(accessToken, ct);
        var itemsTask = purchasingApiClient.GetApprovalConfigsAsync(accessToken, new ApprovalConfigPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DocumentType = documentType,
            Level = level,
            MinAmountFrom = minAmountFrom,
            MinAmountTo = minAmountTo,
            MaxAmountFrom = maxAmountFrom,
            MaxAmountTo = maxAmountTo,
            ApproverEmployeeId = approverEmployeeId,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(approverOptionsTask, itemsTask);

        ViewData["Title"] = "Approval Configs";
        ViewData["Breadcrumb"] = "Purchasing / Approval Configs";

        return View("ApprovalConfigs/Index", new PurchasingApprovalConfigsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DocumentTypeFilter = documentType,
            LevelFilter = level,
            MinAmountFromFilter = minAmountFrom,
            MinAmountToFilter = minAmountTo,
            MaxAmountFromFilter = maxAmountFrom,
            MaxAmountToFilter = maxAmountTo,
            ApproverEmployeeIdFilter = approverEmployeeId,
            IsActiveFilter = isActive,
            ApproverOptions = await approverOptionsTask,
            Items = await itemsTask ?? PagedResult<ApprovalConfigDto>.Create([], 0, normalizedPage, normalizedPageSize)
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

        var model = new PurchasingApprovalConfigEditViewModel
        {
            IsActive = true,
            ApproverOptions = await GetApproverOptionsAsync(accessToken, ct)
        };

        ViewData["Title"] = "Create Approval Config";
        ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Create";

        return View("ApprovalConfigs/Create", model);
    }
    [HttpPost("approval-configs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApprovalConfig(PurchasingApprovalConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.ApproverOptions = await GetApproverOptionsAsync(accessToken, ct);

        if (model.MaxAmount.HasValue && model.MaxAmount.Value < model.MinAmount)
        {
            ModelState.AddModelError(nameof(model.MaxAmount), "Maximum amount must be greater than or equal to minimum amount.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Approval Config";
            ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Create";
            return View("ApprovalConfigs/Create", model);
        }

        var created = await purchasingApiClient.CreateApprovalConfigAsync(accessToken, MapApprovalConfigDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create approval config.");
            ViewData["Title"] = "Create Approval Config";
            ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Create";
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

        var item = await purchasingApiClient.GetApprovalConfigByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new PurchasingApprovalConfigEditViewModel
        {
            Id = item.Id,
            DocumentType = item.DocumentType,
            Level = item.Level,
            MinAmount = item.MinAmount,
            MaxAmount = item.MaxAmount,
            ApproverEmployeeId = item.ApproverEmployeeId,
            Notes = item.Notes,
            IsActive = item.IsActive,
            ApproverOptions = await GetApproverOptionsAsync(accessToken, ct)
        };

        ViewData["Title"] = "Edit Approval Config";
        ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Edit";

        return View("ApprovalConfigs/Edit", model);
    }

    [HttpPost("approval-configs/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditApprovalConfig(int id, PurchasingApprovalConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        model.ApproverOptions = await GetApproverOptionsAsync(accessToken, ct);

        if (model.MaxAmount.HasValue && model.MaxAmount.Value < model.MinAmount)
        {
            ModelState.AddModelError(nameof(model.MaxAmount), "Maximum amount must be greater than or equal to minimum amount.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Approval Config";
            ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Edit";
            return View("ApprovalConfigs/Edit", model);
        }

        var updated = await purchasingApiClient.UpdateApprovalConfigAsync(accessToken, id, MapApprovalConfigDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update approval config.");
            ViewData["Title"] = "Edit Approval Config";
            ViewData["Breadcrumb"] = "Purchasing / Approval Configs / Edit";
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

        var deleted = await purchasingApiClient.DeleteApprovalConfigAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Approval config deleted."
            : "Failed to delete approval config.";

        return RedirectToAction(nameof(ApprovalConfigs));
    }

    [HttpGet("buyer-groups")]
    public async Task<IActionResult> BuyerGroups(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? buyerEmployeeId = null,
        int? itemCategoryId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "buyeremployeename", "mappedcategorycount", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var buyerOptionsTask = GetApproverOptionsAsync(accessToken, ct);
        var itemCategoryOptionsTask = GetItemCategoryOptionsAsync(accessToken, ct);
        var itemsTask = purchasingApiClient.GetBuyerGroupsAsync(accessToken, new BuyerGroupPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            BuyerEmployeeId = buyerEmployeeId,
            ItemCategoryId = itemCategoryId,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(buyerOptionsTask, itemCategoryOptionsTask, itemsTask);

        ViewData["Title"] = "Buyer Groups";
        ViewData["Breadcrumb"] = "Purchasing / Buyer Groups";

        return View("BuyerGroups/Index", new PurchasingBuyerGroupsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            BuyerEmployeeIdFilter = buyerEmployeeId,
            ItemCategoryIdFilter = itemCategoryId,
            IsActiveFilter = isActive,
            BuyerOptions = await buyerOptionsTask,
            ItemCategoryOptions = await itemCategoryOptionsTask,
            Items = await itemsTask ?? PagedResult<BuyerGroupDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("buyer-groups/create")]
    public async Task<IActionResult> CreateBuyerGroup(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var buyerOptionsTask = GetApproverOptionsAsync(accessToken, ct);
        var itemCategoryOptionsTask = GetItemCategoryOptionsAsync(accessToken, ct);

        await Task.WhenAll(buyerOptionsTask, itemCategoryOptionsTask);

        ViewData["Title"] = "Create Buyer Group";
        ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Create";

        return View("BuyerGroups/Create", new PurchasingBuyerGroupEditViewModel
        {
            IsActive = true,
            BuyerOptions = await buyerOptionsTask,
            ItemCategoryOptions = await itemCategoryOptionsTask
        });
    }

    [HttpPost("buyer-groups/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBuyerGroup(PurchasingBuyerGroupEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateBuyerGroupFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Buyer Group";
            ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Create";
            return View("BuyerGroups/Create", model);
        }

        var created = await purchasingApiClient.CreateBuyerGroupAsync(accessToken, MapBuyerGroupDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create buyer group.");
            ViewData["Title"] = "Create Buyer Group";
            ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Create";
            return View("BuyerGroups/Create", model);
        }

        TempData["SuccessMessage"] = "Buyer group created.";
        return RedirectToAction(nameof(BuyerGroups));
    }
    [HttpGet("buyer-groups/edit/{id:int}")]
    public async Task<IActionResult> EditBuyerGroup(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await purchasingApiClient.GetBuyerGroupByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new PurchasingBuyerGroupEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            BuyerEmployeeId = item.BuyerEmployeeId,
            Description = item.Description,
            IsActive = item.IsActive,
            ItemCategoryIds = item.ItemCategoryIds?.ToList() ?? []
        };

        await PopulateBuyerGroupFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Buyer Group";
        ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Edit";

        return View("BuyerGroups/Edit", model);
    }

    [HttpPost("buyer-groups/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBuyerGroup(int id, PurchasingBuyerGroupEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateBuyerGroupFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Buyer Group";
            ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Edit";
            return View("BuyerGroups/Edit", model);
        }

        var updated = await purchasingApiClient.UpdateBuyerGroupAsync(accessToken, id, MapBuyerGroupDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update buyer group.");
            ViewData["Title"] = "Edit Buyer Group";
            ViewData["Breadcrumb"] = "Purchasing / Buyer Groups / Edit";
            return View("BuyerGroups/Edit", model);
        }

        TempData["SuccessMessage"] = "Buyer group updated.";
        return RedirectToAction(nameof(BuyerGroups));
    }

    [HttpPost("buyer-groups/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBuyerGroup(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await purchasingApiClient.DeleteBuyerGroupAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Buyer group deleted."
            : "Failed to delete buyer group.";

        return RedirectToAction(nameof(BuyerGroups));
    }

    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? vendorCategoryId = null,
        int? buyerGroupId = null,
        bool? isApprovedVendor = null,
        decimal? performanceScoreFrom = null,
        decimal? performanceScoreTo = null,
        int? paymentTermsFrom = null,
        int? paymentTermsTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "vendorcategoryname", "buyergroupname", "paymenttermsdays", "performancescore", "isapprovedvendor", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedScoreFrom, normalizedScoreTo) = NormalizeDecimalRange(performanceScoreFrom, performanceScoreTo);
        var (normalizedTermsFrom, normalizedTermsTo) = NormalizeIntRange(paymentTermsFrom, paymentTermsTo);

        var vendorCategoryOptionsTask = purchasingApiClient.GetVendorCategoryOptionsAsync(accessToken, ct);
        var buyerGroupOptionsTask = purchasingApiClient.GetBuyerGroupOptionsAsync(accessToken, ct);
        var itemsTask = purchasingApiClient.GetVendorsAsync(accessToken, new PurchasingVendorPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            VendorCategoryId = vendorCategoryId,
            BuyerGroupId = buyerGroupId,
            IsApprovedVendor = isApprovedVendor,
            PerformanceScoreFrom = normalizedScoreFrom,
            PerformanceScoreTo = normalizedScoreTo,
            PaymentTermsFrom = normalizedTermsFrom,
            PaymentTermsTo = normalizedTermsTo,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(vendorCategoryOptionsTask, buyerGroupOptionsTask, itemsTask);

        ViewData["Title"] = "Vendors";
        ViewData["Breadcrumb"] = "Purchasing / Vendors";

        return View("Vendors/Index", new PurchasingVendorsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            VendorCategoryIdFilter = vendorCategoryId,
            BuyerGroupIdFilter = buyerGroupId,
            IsApprovedVendorFilter = isApprovedVendor,
            PerformanceScoreFromFilter = normalizedScoreFrom,
            PerformanceScoreToFilter = normalizedScoreTo,
            PaymentTermsFromFilter = normalizedTermsFrom,
            PaymentTermsToFilter = normalizedTermsTo,
            IsActiveFilter = isActive,
            VendorCategoryOptions = await vendorCategoryOptionsTask,
            BuyerGroupOptions = await buyerGroupOptionsTask,
            Items = await itemsTask ?? PagedResult<PurchasingVendorDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("vendors/{id:int}")]
    public async Task<IActionResult> VendorDetail(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var vendor = await purchasingApiClient.GetVendorByIdAsync(accessToken, id, ct);
        if (vendor is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Vendor Detail";
        ViewData["Breadcrumb"] = "Purchasing / Vendors / Detail";

        return View("Vendors/Detail", new PurchasingVendorDetailViewModel
        {
            Vendor = vendor
        });
    }

    [HttpPost("vendors/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVendor(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var updated = await purchasingApiClient.SetApprovedVendorAsync(accessToken, id, true, DateOnly.FromDateTime(DateTime.UtcNow), ct: ct);
        TempData[updated is not null ? "SuccessMessage" : "ErrorMessage"] = updated is not null
            ? "Vendor approved."
            : "Failed to approve vendor.";

        return RedirectToAction(nameof(Vendors));
    }

    [HttpPost("vendors/reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectVendor(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var updated = await purchasingApiClient.SetApprovedVendorAsync(accessToken, id, false, null, ct);
        TempData[updated is not null ? "SuccessMessage" : "ErrorMessage"] = updated is not null
            ? "Vendor approval removed."
            : "Failed to update vendor approval.";

        return RedirectToAction(nameof(Vendors));
    }
    private IActionResult? RequireAccessToken(out string accessToken, bool includeReturnUrl = true)
    {
        accessToken = User.FindFirstValue("access_token") ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        return includeReturnUrl
            ? RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString })
            : RedirectToAction("Login", "Auth");
    }

    private async Task PopulateBuyerGroupFormOptionsAsync(string accessToken, PurchasingBuyerGroupEditViewModel model, CancellationToken ct)
    {
        var buyerOptionsTask = GetApproverOptionsAsync(accessToken, ct);
        var itemCategoryOptionsTask = GetItemCategoryOptionsAsync(accessToken, ct);

        await Task.WhenAll(buyerOptionsTask, itemCategoryOptionsTask);

        model.BuyerOptions = await buyerOptionsTask;
        model.ItemCategoryOptions = await itemCategoryOptionsTask;
    }

    private async Task<IReadOnlyList<LookupDto>> GetApproverOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        return options
            .OrderBy(x => x.Name)
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetItemCategoryOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        return options
            .OrderBy(x => x.Label)
            .ToList();
    }

    private static VendorCategoryDto MapVendorCategoryDto(PurchasingVendorCategoryEditViewModel model)
    {
        return new VendorCategoryDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
    }

    private static ApprovalConfigDto MapApprovalConfigDto(PurchasingApprovalConfigEditViewModel model)
    {
        return new ApprovalConfigDto
        {
            Id = model.Id ?? 0,
            DocumentType = model.DocumentType,
            Level = model.Level,
            MinAmount = model.MinAmount,
            MaxAmount = model.MaxAmount,
            ApproverEmployeeId = model.ApproverEmployeeId,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static BuyerGroupDto MapBuyerGroupDto(PurchasingBuyerGroupEditViewModel model)
    {
        return new BuyerGroupDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            BuyerEmployeeId = model.BuyerEmployeeId,
            Description = model.Description,
            IsActive = model.IsActive,
            ItemCategoryIds = model.ItemCategoryIds
                .Where(x => x > 0)
                .Distinct()
                .ToList()
        };
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string NormalizeSortBy(string? sortBy, string defaultSortBy, params string[] allowedSortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return defaultSortBy;
        }

        var normalized = sortBy.Trim().ToLowerInvariant();
        return allowedSortBy.Contains(normalized, StringComparer.OrdinalIgnoreCase) ? normalized : defaultSortBy;
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (decimal? From, decimal? To) NormalizeDecimalRange(decimal? from, decimal? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static (int? From, int? To) NormalizeIntRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }
}

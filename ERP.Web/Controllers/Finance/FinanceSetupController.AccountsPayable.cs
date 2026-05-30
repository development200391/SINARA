
using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        string? taxId = null,
        string? contactPerson = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "taxid", "contactperson", "paymenttermsdays", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var normalizedTaxId = NormalizeText(taxId);
        var normalizedContactPerson = NormalizeText(contactPerson);
        var (normalizedPaymentTermsFrom, normalizedPaymentTermsTo) = NormalizeIntRange(paymentTermsFrom, paymentTermsTo);

        var items = await financeApiClient.GetVendorsAsync(accessToken, new VendorPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            TaxId = normalizedTaxId,
            ContactPerson = normalizedContactPerson,
            PaymentTermsFrom = normalizedPaymentTermsFrom,
            PaymentTermsTo = normalizedPaymentTermsTo,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Vendors";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors";

        return View("Vendors/Index", new FinanceVendorsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TaxIdFilter = normalizedTaxId,
            ContactPersonFilter = normalizedContactPerson,
            PaymentTermsFromFilter = normalizedPaymentTermsFrom,
            PaymentTermsToFilter = normalizedPaymentTermsTo,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<VendorDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("vendors/create")]
    public async Task<IActionResult> CreateVendor(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceVendorEditViewModel();
        await PopulateVendorFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Vendor";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Create";

        return View("Vendors/Create", model);
    }

    [HttpPost("vendors/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVendor(FinanceVendorEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeVendorForm(model);
        await PopulateVendorFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Vendor";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Create";
            return View("Vendors/Create", model);
        }

        var created = await financeApiClient.CreateVendorAsync(accessToken, MapVendorRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create vendor.");
            ViewData["Title"] = "Create Vendor";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Create";
            return View("Vendors/Create", model);
        }

        TempData["SuccessMessage"] = "Vendor created.";
        return RedirectToAction(nameof(Vendors));
    }

    [HttpGet("vendors/edit/{id:int}")]
    public async Task<IActionResult> EditVendor(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var vendor = await financeApiClient.GetVendorByIdAsync(accessToken, id, ct);
        if (vendor is null)
        {
            return NotFound();
        }

        var model = new FinanceVendorEditViewModel
        {
            Id = vendor.Id,
            Code = vendor.Code,
            Name = vendor.Name,
            TaxId = vendor.TaxId,
            Address = vendor.Address,
            Phone = vendor.Phone,
            Email = vendor.Email,
            ContactPerson = vendor.ContactPerson,
            PaymentTermsDays = vendor.PaymentTermsDays,
            DefaultAccountId = vendor.DefaultAccountId,
            DefaultTaxCodeId = vendor.DefaultTaxCodeId,
            BankName = vendor.BankName,
            BankAccountNo = vendor.BankAccountNo,
            IsActive = vendor.IsActive
        };

        await PopulateVendorFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Vendor";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Edit";

        return View("Vendors/Edit", model);
    }

    [HttpPost("vendors/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVendor(int id, FinanceVendorEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeVendorForm(model);
        await PopulateVendorFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Vendor";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Edit";
            return View("Vendors/Edit", model);
        }

        var updated = await financeApiClient.UpdateVendorAsync(accessToken, id, MapVendorRequest(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update vendor.");
            ViewData["Title"] = "Edit Vendor";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / Vendors / Edit";
            return View("Vendors/Edit", model);
        }

        TempData["SuccessMessage"] = "Vendor updated.";
        return RedirectToAction(nameof(Vendors));
    }

    [HttpPost("vendors/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVendor(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteVendorAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Vendor deleted." : "Failed to delete vendor.";
        return RedirectToAction(nameof(Vendors));
    }

    [HttpGet("ap/invoices")]
    public async Task<IActionResult> ApInvoices(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "invoicedate",
        string? sortDirection = "desc",
        string? invoiceNo = null,
        string? vendorInvoiceNo = null,
        int? vendorId = null,
        int? periodId = null,
        DateOnly? invoiceDateFrom = null,
        DateOnly? invoiceDateTo = null,
        DateOnly? dueDateFrom = null,
        DateOnly? dueDateTo = null,
        FinanceApInvoiceStatus? status = null,
        decimal? outstandingFrom = null,
        decimal? outstandingTo = null,
        bool? isOverdue = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "invoicedate", "invoiceno", "vendorname", "invoicedate", "duedate", "totalamount", "outstandingamount", "status", "approvedat", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedInvoiceNo = NormalizeText(invoiceNo);
        var normalizedVendorInvoiceNo = NormalizeText(vendorInvoiceNo);
        var (normalizedInvoiceDateFrom, normalizedInvoiceDateTo) = NormalizeDateRange(invoiceDateFrom, invoiceDateTo);
        var (normalizedDueDateFrom, normalizedDueDateTo) = NormalizeDateRange(dueDateFrom, dueDateTo);
        var (normalizedOutstandingFrom, normalizedOutstandingTo) = NormalizeDecimalRange(outstandingFrom, outstandingTo);

        var itemsTask = financeApiClient.GetApInvoicesAsync(accessToken, new ApInvoicePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            InvoiceNo = normalizedInvoiceNo,
            VendorInvoiceNo = normalizedVendorInvoiceNo,
            VendorId = vendorId,
            PeriodId = periodId,
            InvoiceDateFrom = normalizedInvoiceDateFrom,
            InvoiceDateTo = normalizedInvoiceDateTo,
            DueDateFrom = normalizedDueDateFrom,
            DueDateTo = normalizedDueDateTo,
            Status = status,
            OutstandingFrom = normalizedOutstandingFrom,
            OutstandingTo = normalizedOutstandingTo,
            IsOverdue = isOverdue
        }, ct);

        var vendorOptionsTask = LoadVendorOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, vendorOptionsTask, periodOptionsTask);

        ViewData["Title"] = "AP Invoices";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices";

        return View("ApInvoices/Index", new FinanceApInvoicesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            InvoiceNoFilter = normalizedInvoiceNo,
            VendorInvoiceNoFilter = normalizedVendorInvoiceNo,
            VendorIdFilter = vendorId,
            PeriodIdFilter = periodId,
            InvoiceDateFromFilter = normalizedInvoiceDateFrom,
            InvoiceDateToFilter = normalizedInvoiceDateTo,
            DueDateFromFilter = normalizedDueDateFrom,
            DueDateToFilter = normalizedDueDateTo,
            StatusFilter = status,
            OutstandingFromFilter = normalizedOutstandingFrom,
            OutstandingToFilter = normalizedOutstandingTo,
            IsOverdueFilter = isOverdue,
            VendorOptions = await vendorOptionsTask,
            PeriodOptions = await periodOptionsTask,
            Items = await itemsTask ?? PagedResult<ApInvoiceDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("ap/invoices/create")]
    public async Task<IActionResult> CreateApInvoice(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceApInvoiceEditViewModel();
        await PopulateApInvoiceFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create AP Invoice";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Create";

        return View("ApInvoices/Create", model);
    }

    [HttpPost("ap/invoices/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApInvoice(FinanceApInvoiceEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeApInvoiceForm(model);
        await PopulateApInvoiceFormOptionsAsync(accessToken, model, ct);
        ValidateApInvoiceForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create AP Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Create";
            return View("ApInvoices/Create", model);
        }

        var created = await financeApiClient.CreateApInvoiceAsync(accessToken, MapApInvoiceRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create AP invoice.");
            ViewData["Title"] = "Create AP Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Create";
            return View("ApInvoices/Create", model);
        }

        TempData["SuccessMessage"] = "AP invoice created.";
        return RedirectToAction(nameof(ApInvoices));
    }

    [HttpGet("ap/invoices/edit/{id:int}")]
    public async Task<IActionResult> EditApInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var invoice = await financeApiClient.GetApInvoiceByIdAsync(accessToken, id, ct);
        if (invoice is null)
        {
            return NotFound();
        }

        var model = new FinanceApInvoiceEditViewModel
        {
            Id = invoice.Id,
            InvoiceNo = invoice.InvoiceNo,
            VendorId = invoice.VendorId,
            VendorInvoiceNo = invoice.VendorInvoiceNo,
            PeriodId = invoice.PeriodId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Description = invoice.Description,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Status = invoice.Status,
            ApprovedAt = invoice.ApprovedAt,
            ApprovedByName = invoice.ApprovedByName,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            OutstandingAmount = invoice.OutstandingAmount,
            IsOverdue = invoice.IsOverdue,
            Lines = invoice.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new FinanceApInvoiceLineEditViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TaxCodeId = x.TaxCodeId,
                    TaxAmount = x.TaxAmount,
                    AccountId = x.AccountId,
                    CostCenterId = x.CostCenterId
                })
                .ToList()
        };

        await PopulateApInvoiceFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit AP Invoice";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Edit";

        return View("ApInvoices/Edit", model);
    }

    [HttpPost("ap/invoices/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditApInvoice(int id, FinanceApInvoiceEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var current = await financeApiClient.GetApInvoiceByIdAsync(accessToken, id, ct);
        if (current is null)
        {
            return NotFound();
        }

        if (current.Status != FinanceApInvoiceStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft AP invoice can be edited.";
            return RedirectToAction(nameof(ApInvoices));
        }

        model.Id = id;
        model.InvoiceNo = current.InvoiceNo;
        model.Status = current.Status;

        NormalizeApInvoiceForm(model);
        await PopulateApInvoiceFormOptionsAsync(accessToken, model, ct);
        ValidateApInvoiceForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit AP Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Edit";
            return View("ApInvoices/Edit", model);
        }

        var updated = await financeApiClient.UpdateApInvoiceAsync(accessToken, id, MapApInvoiceRequest(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update AP invoice.");
            ViewData["Title"] = "Edit AP Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Invoices / Edit";
            return View("ApInvoices/Edit", model);
        }

        TempData["SuccessMessage"] = "AP invoice updated.";
        return RedirectToAction(nameof(ApInvoices));
    }

    [HttpPost("ap/invoices/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveApInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var approved = await financeApiClient.ApproveApInvoiceAsync(accessToken, id, ct);
        TempData[approved is null ? "ErrorMessage" : "SuccessMessage"] = approved is null
            ? "Failed to approve AP invoice."
            : "AP invoice approved.";

        return RedirectToAction(nameof(ApInvoices));
    }

    [HttpPost("ap/invoices/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteApInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteApInvoiceAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "AP invoice deleted." : "Failed to delete AP invoice.";

        return RedirectToAction(nameof(ApInvoices));
    }

    [HttpGet("ap/payments")]
    public async Task<IActionResult> ApPayments(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "paymentdate",
        string? sortDirection = "desc",
        string? paymentNo = null,
        int? vendorId = null,
        DateOnly? paymentDateFrom = null,
        DateOnly? paymentDateTo = null,
        FinanceApPaymentMethod? paymentMethod = null,
        decimal? amountFrom = null,
        decimal? amountTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "paymentdate", "paymentno", "vendorname", "paymentdate", "amount", "paymentmethod", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedPaymentNo = NormalizeText(paymentNo);
        var (normalizedPaymentDateFrom, normalizedPaymentDateTo) = NormalizeDateRange(paymentDateFrom, paymentDateTo);
        var (normalizedAmountFrom, normalizedAmountTo) = NormalizeDecimalRange(amountFrom, amountTo);

        var itemsTask = financeApiClient.GetApPaymentsAsync(accessToken, new ApPaymentPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PaymentNo = normalizedPaymentNo,
            VendorId = vendorId,
            PaymentDateFrom = normalizedPaymentDateFrom,
            PaymentDateTo = normalizedPaymentDateTo,
            PaymentMethod = paymentMethod,
            AmountFrom = normalizedAmountFrom,
            AmountTo = normalizedAmountTo
        }, ct);

        var vendorOptionsTask = LoadVendorOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, vendorOptionsTask);

        ViewData["Title"] = "AP Payments";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Payments";

        return View("ApPayments/Index", new FinanceApPaymentsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PaymentNoFilter = normalizedPaymentNo,
            VendorIdFilter = vendorId,
            PaymentDateFromFilter = normalizedPaymentDateFrom,
            PaymentDateToFilter = normalizedPaymentDateTo,
            PaymentMethodFilter = paymentMethod,
            AmountFromFilter = normalizedAmountFrom,
            AmountToFilter = normalizedAmountTo,
            VendorOptions = await vendorOptionsTask,
            Items = await itemsTask ?? PagedResult<ApPaymentDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("ap/payments/create")]
    public async Task<IActionResult> CreateApPayment(int? vendorId = null, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceApPaymentCreateViewModel
        {
            VendorId = vendorId is > 0 ? vendorId.Value : 0
        };

        await PopulateApPaymentFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create AP Payment";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Payments / Create";

        return View("ApPayments/Create", model);
    }

    [HttpPost("ap/payments/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApPayment(FinanceApPaymentCreateViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeApPaymentForm(model);
        await PopulateApPaymentFormOptionsAsync(accessToken, model, ct);
        ValidateApPaymentForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create AP Payment";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Payments / Create";
            return View("ApPayments/Create", model);
        }

        var created = await financeApiClient.CreateApPaymentAsync(accessToken, MapApPaymentRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create AP payment.");
            ViewData["Title"] = "Create AP Payment";
            ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Payments / Create";
            return View("ApPayments/Create", model);
        }

        TempData["SuccessMessage"] = "AP payment created.";
        return RedirectToAction(nameof(ApPayments));
    }

    [HttpGet("ap/aging")]
    public async Task<IActionResult> ApAging(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "totaloutstanding",
        string? sortDirection = "desc",
        int? vendorId = null,
        DateOnly? asOfDate = null,
        decimal? outstandingMin = null,
        decimal? outstandingMax = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "totaloutstanding", "vendorcode", "vendorname", "currentamount", "bucket1to30", "bucket31to60", "bucket61to90", "bucketover90", "totaloutstanding");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedAsOfDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var (normalizedOutstandingMin, normalizedOutstandingMax) = NormalizeDecimalRange(outstandingMin, outstandingMax);

        var itemsTask = financeApiClient.GetApAgingAsync(accessToken, new ApAgingPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            VendorId = vendorId,
            AsOfDate = normalizedAsOfDate,
            OutstandingMin = normalizedOutstandingMin,
            OutstandingMax = normalizedOutstandingMax
        }, ct);

        var vendorOptionsTask = LoadVendorOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, vendorOptionsTask);

        ViewData["Title"] = "AP Aging";
        ViewData["Breadcrumb"] = "Finance / Accounts Payable / AP Aging";

        return View("ApAging/Index", new FinanceApAgingIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            VendorIdFilter = vendorId,
            AsOfDateFilter = normalizedAsOfDate,
            OutstandingMinFilter = normalizedOutstandingMin,
            OutstandingMaxFilter = normalizedOutstandingMax,
            VendorOptions = await vendorOptionsTask,
            Items = await itemsTask ?? PagedResult<ApAgingRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private async Task PopulateVendorFormOptionsAsync(string accessToken, FinanceVendorEditViewModel model, CancellationToken ct)
    {
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var taxCodeOptionsTask = LoadTaxCodeOptionsAsync(accessToken, ct);

        await Task.WhenAll(accountOptionsTask, taxCodeOptionsTask);

        var accountOptions = await accountOptionsTask;
        var taxCodeOptions = await taxCodeOptionsTask;

        if (model.DefaultAccountId.HasValue && accountOptions.All(x => x.Id != model.DefaultAccountId.Value))
        {
            model.DefaultAccountId = null;
        }

        if (model.DefaultTaxCodeId.HasValue && taxCodeOptions.All(x => x.Id != model.DefaultTaxCodeId.Value))
        {
            model.DefaultTaxCodeId = null;
        }

        model.AccountOptions = accountOptions;
        model.TaxCodeOptions = taxCodeOptions;
    }

    private static void NormalizeVendorForm(FinanceVendorEditViewModel model)
    {
        model.Code = string.IsNullOrWhiteSpace(model.Code) ? string.Empty : model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.TaxId = NormalizeText(model.TaxId);
        model.Address = NormalizeText(model.Address);
        model.Phone = NormalizeText(model.Phone);
        model.Email = NormalizeText(model.Email);
        model.ContactPerson = NormalizeText(model.ContactPerson);
        model.BankName = NormalizeText(model.BankName);
        model.BankAccountNo = NormalizeText(model.BankAccountNo);

        model.DefaultAccountId = model.DefaultAccountId is > 0 ? model.DefaultAccountId : null;
        model.DefaultTaxCodeId = model.DefaultTaxCodeId is > 0 ? model.DefaultTaxCodeId : null;
    }

    private static VendorDto MapVendorRequest(FinanceVendorEditViewModel model)
    {
        return new VendorDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            TaxId = model.TaxId,
            Address = model.Address,
            Phone = model.Phone,
            Email = model.Email,
            ContactPerson = model.ContactPerson,
            PaymentTermsDays = model.PaymentTermsDays,
            DefaultAccountId = model.DefaultAccountId,
            DefaultTaxCodeId = model.DefaultTaxCodeId,
            BankName = model.BankName,
            BankAccountNo = model.BankAccountNo,
            IsActive = model.IsActive
        };
    }

    private async Task PopulateApInvoiceFormOptionsAsync(string accessToken, FinanceApInvoiceEditViewModel model, CancellationToken ct)
    {
        var vendorOptionsTask = LoadVendorOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        var taxCodeOptionsTask = LoadTaxCodeOptionsAsync(accessToken, ct);

        await Task.WhenAll(vendorOptionsTask, periodOptionsTask, currencyOptionsTask, accountOptionsTask, costCenterOptionsTask, taxCodeOptionsTask);

        model.VendorOptions = await vendorOptionsTask;
        model.PeriodOptions = await periodOptionsTask;
        model.CurrencyOptions = await currencyOptionsTask;
        model.AccountOptions = await accountOptionsTask;
        model.CostCenterOptions = await costCenterOptionsTask;
        model.TaxCodeOptions = await taxCodeOptionsTask;

        if (model.VendorId <= 0 || model.VendorOptions.All(x => x.Id != model.VendorId))
        {
            model.VendorId = model.VendorOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (model.PeriodId <= 0 || model.PeriodOptions.All(x => x.Id != model.PeriodId))
        {
            model.PeriodId = model.PeriodOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (string.IsNullOrWhiteSpace(model.CurrencyCode) || model.CurrencyOptions.All(x => !string.Equals(x.Code, model.CurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            model.CurrencyCode = model.CurrencyOptions.FirstOrDefault()?.Code ?? "IDR";
        }

        if (model.ExchangeRate <= 0)
        {
            model.ExchangeRate = 1m;
        }

        if (model.Lines.Count == 0)
        {
            model.Lines = [new FinanceApInvoiceLineEditViewModel()];
        }
    }

    private static void NormalizeApInvoiceForm(FinanceApInvoiceEditViewModel model)
    {
        model.VendorInvoiceNo = NormalizeText(model.VendorInvoiceNo);
        model.Description = NormalizeText(model.Description);
        model.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode)
            ? "IDR"
            : model.CurrencyCode.Trim().ToUpperInvariant();

        if (model.ExchangeRate <= 0)
        {
            model.ExchangeRate = 1m;
        }

        var normalizedLines = model.Lines
            .Where(x =>
                x.AccountId > 0 ||
                x.CostCenterId.HasValue ||
                x.TaxCodeId.HasValue ||
                !string.IsNullOrWhiteSpace(x.Description) ||
                x.Quantity > 0 ||
                x.UnitPrice > 0 ||
                x.TaxAmount > 0)
            .Select(x => new FinanceApInvoiceLineEditViewModel
            {
                Id = x.Id,
                Description = NormalizeText(x.Description) ?? string.Empty,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TaxCodeId = x.TaxCodeId is > 0 ? x.TaxCodeId : null,
                TaxAmount = x.TaxAmount,
                AccountId = x.AccountId,
                CostCenterId = x.CostCenterId is > 0 ? x.CostCenterId : null
            })
            .ToList();

        model.Lines = normalizedLines;

        if (model.Lines.Count == 0)
        {
            model.Lines = [new FinanceApInvoiceLineEditViewModel()];
        }
    }

    private void ValidateApInvoiceForm(FinanceApInvoiceEditViewModel model)
    {
        if (model.DueDate < model.InvoiceDate)
        {
            ModelState.AddModelError(nameof(model.DueDate), "Due date must be greater than or equal to invoice date.");
        }

        if (model.ExchangeRate <= 0)
        {
            ModelState.AddModelError(nameof(model.ExchangeRate), "Exchange rate must be greater than 0.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "At least one invoice line is required.");
            return;
        }

        for (var index = 0; index < model.Lines.Count; index++)
        {
            var line = model.Lines[index];

            if (line.AccountId <= 0)
            {
                ModelState.AddModelError($"Lines[{index}].AccountId", $"Account is required at line {index + 1}.");
            }

            if (string.IsNullOrWhiteSpace(line.Description))
            {
                ModelState.AddModelError($"Lines[{index}].Description", $"Description is required at line {index + 1}.");
            }

            if (line.Quantity <= 0)
            {
                ModelState.AddModelError($"Lines[{index}].Quantity", $"Quantity must be greater than 0 at line {index + 1}.");
            }

            if (line.UnitPrice < 0)
            {
                ModelState.AddModelError($"Lines[{index}].UnitPrice", $"Unit price cannot be negative at line {index + 1}.");
            }

            if (line.TaxAmount < 0)
            {
                ModelState.AddModelError($"Lines[{index}].TaxAmount", $"Tax amount cannot be negative at line {index + 1}.");
            }
        }
    }

    private static ApInvoiceDto MapApInvoiceRequest(FinanceApInvoiceEditViewModel model)
    {
        var lines = model.Lines
            .Select((line, index) => new ApInvoiceLineDto
            {
                Id = line.Id ?? 0,
                LineNo = index + 1,
                Description = line.Description.Trim(),
                Quantity = decimal.Round(line.Quantity, 4, MidpointRounding.AwayFromZero),
                UnitPrice = decimal.Round(line.UnitPrice, 4, MidpointRounding.AwayFromZero),
                Amount = decimal.Round(line.Quantity * line.UnitPrice, 4, MidpointRounding.AwayFromZero),
                TaxCodeId = line.TaxCodeId,
                TaxAmount = decimal.Round(line.TaxAmount, 4, MidpointRounding.AwayFromZero),
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId
            })
            .ToList();

        return new ApInvoiceDto
        {
            Id = model.Id ?? 0,
            InvoiceNo = model.InvoiceNo,
            VendorId = model.VendorId,
            VendorInvoiceNo = model.VendorInvoiceNo,
            PeriodId = model.PeriodId,
            InvoiceDate = model.InvoiceDate,
            DueDate = model.DueDate,
            Description = model.Description,
            CurrencyCode = model.CurrencyCode,
            ExchangeRate = model.ExchangeRate,
            Lines = lines
        };
    }

    private async Task PopulateApPaymentFormOptionsAsync(string accessToken, FinanceApPaymentCreateViewModel model, CancellationToken ct)
    {
        var vendorOptionsTask = LoadVendorOptionsAsync(accessToken, ct);
        var bankAccountOptionsTask = LoadBankAccountOptionsAsync(accessToken, ct);
        var outstandingOptionsTask = LoadOutstandingInvoiceOptionsAsync(accessToken, ct);

        await Task.WhenAll(vendorOptionsTask, bankAccountOptionsTask, outstandingOptionsTask);

        var vendorOptions = await vendorOptionsTask;
        var bankAccountOptions = await bankAccountOptionsTask;
        var outstandingOptions = await outstandingOptionsTask;

        if (model.VendorId <= 0 || vendorOptions.All(x => x.Id != model.VendorId))
        {
            model.VendorId = vendorOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (model.BankAccountId <= 0 || bankAccountOptions.All(x => x.Id != model.BankAccountId))
        {
            model.BankAccountId = bankAccountOptions.FirstOrDefault()?.Id ?? 0;
        }

        var validInvoiceMap = outstandingOptions.ToDictionary(x => x.InvoiceId, x => x);

        model.Applications = model.Applications
            .Where(x => x.InvoiceId > 0 && x.AppliedAmount > 0)
            .Where(x => validInvoiceMap.TryGetValue(x.InvoiceId, out var option) && option.VendorId == model.VendorId)
            .GroupBy(x => x.InvoiceId)
            .Select(x => new FinanceApPaymentApplicationEditViewModel
            {
                InvoiceId = x.Key,
                AppliedAmount = decimal.Round(x.Sum(y => y.AppliedAmount), 4, MidpointRounding.AwayFromZero)
            })
            .ToList();

        model.VendorOptions = vendorOptions;
        model.BankAccountOptions = bankAccountOptions;
        model.OutstandingInvoiceOptions = outstandingOptions;
    }

    private static void NormalizeApPaymentForm(FinanceApPaymentCreateViewModel model)
    {
        model.ReferenceNo = NormalizeText(model.ReferenceNo);
        model.Notes = NormalizeText(model.Notes);

        var normalizedApplications = model.Applications
            .Where(x => x.InvoiceId > 0 || x.AppliedAmount > 0)
            .Select(x => new FinanceApPaymentApplicationEditViewModel
            {
                InvoiceId = x.InvoiceId,
                AppliedAmount = x.AppliedAmount
            })
            .ToList();

        model.Applications = normalizedApplications;

        var totalApplied = model.Applications
            .Where(x => x.InvoiceId > 0 && x.AppliedAmount > 0)
            .Sum(x => decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero));

        if (model.Amount <= 0 && totalApplied > 0)
        {
            model.Amount = totalApplied;
        }
    }

    private void ValidateApPaymentForm(FinanceApPaymentCreateViewModel model)
    {
        if (model.VendorId <= 0)
        {
            ModelState.AddModelError(nameof(model.VendorId), "Vendor is required.");
        }

        if (model.BankAccountId <= 0)
        {
            ModelState.AddModelError(nameof(model.BankAccountId), "Bank account is required.");
        }

        if (model.Applications.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "At least one payment application is required.");
            return;
        }

        var duplicateInvoice = model.Applications
            .Where(x => x.InvoiceId > 0)
            .GroupBy(x => x.InvoiceId)
            .Any(x => x.Count() > 1);

        if (duplicateInvoice)
        {
            ModelState.AddModelError(string.Empty, "Duplicate invoice in applications is not allowed.");
        }

        decimal totalApplied = 0m;
        for (var index = 0; index < model.Applications.Count; index++)
        {
            var application = model.Applications[index];

            if (application.InvoiceId <= 0)
            {
                ModelState.AddModelError($"Applications[{index}].InvoiceId", $"Invoice is required at row {index + 1}.");
            }

            if (application.AppliedAmount <= 0)
            {
                ModelState.AddModelError($"Applications[{index}].AppliedAmount", $"Applied amount must be greater than 0 at row {index + 1}.");
            }

            if (application.AppliedAmount > 0)
            {
                totalApplied += decimal.Round(application.AppliedAmount, 4, MidpointRounding.AwayFromZero);
            }
        }

        if (model.Amount <= 0)
        {
            ModelState.AddModelError(nameof(model.Amount), "Amount must be greater than 0.");
        }

        if (model.Amount > 0 && totalApplied > 0)
        {
            var diff = Math.Abs(model.Amount - totalApplied);
            if (diff > 0.0001m)
            {
                ModelState.AddModelError(nameof(model.Amount), "Amount must equal total applied amount.");
            }
        }
    }

    private static ApPaymentDto MapApPaymentRequest(FinanceApPaymentCreateViewModel model)
    {
        return new ApPaymentDto
        {
            VendorId = model.VendorId,
            PaymentDate = model.PaymentDate,
            Amount = decimal.Round(model.Amount, 4, MidpointRounding.AwayFromZero),
            PaymentMethod = model.PaymentMethod,
            BankAccountId = model.BankAccountId,
            ReferenceNo = model.ReferenceNo,
            Notes = model.Notes,
            Applications = model.Applications
                .Select(x => new ApPaymentApplicationDto
                {
                    InvoiceId = x.InvoiceId,
                    AppliedAmount = decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero)
                })
                .ToList()
        };
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadVendorOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetVendorsAsync(accessToken, new VendorPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadTaxCodeOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetTaxCodesAsync(accessToken, new TaxCodePagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadBankAccountOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetAccountsAsync(accessToken, new AccountPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc",
            IsBankAccount = true,
            IsActive = true
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FinanceIdOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<FinanceApInvoiceOutstandingOptionViewModel>> LoadOutstandingInvoiceOptionsAsync(string accessToken, CancellationToken ct)
    {
        var approvedTask = financeApiClient.GetApInvoicesAsync(accessToken, new ApInvoicePagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "duedate",
            SortDirection = "asc",
            Status = FinanceApInvoiceStatus.Approved,
            OutstandingFrom = 0.0001m
        }, ct);

        var partiallyPaidTask = financeApiClient.GetApInvoicesAsync(accessToken, new ApInvoicePagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "duedate",
            SortDirection = "asc",
            Status = FinanceApInvoiceStatus.PartiallyPaid,
            OutstandingFrom = 0.0001m
        }, ct);

        await Task.WhenAll(approvedTask, partiallyPaidTask);

        var rows = new List<ApInvoiceDto>();
        var approved = await approvedTask;
        var partiallyPaid = await partiallyPaidTask;

        if (approved?.Items is { Count: > 0 })
        {
            rows.AddRange(approved.Items);
        }

        if (partiallyPaid?.Items is { Count: > 0 })
        {
            rows.AddRange(partiallyPaid.Items);
        }

        return rows
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .Where(x => x.OutstandingAmount > 0)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.InvoiceNo)
            .Select(x => new FinanceApInvoiceOutstandingOptionViewModel
            {
                VendorId = x.VendorId,
                InvoiceId = x.Id,
                InvoiceNo = x.InvoiceNo,
                InvoiceDate = x.InvoiceDate,
                DueDate = x.DueDate,
                OutstandingAmount = x.OutstandingAmount,
                Label = $"{x.InvoiceNo} | {x.VendorCode} - {x.VendorName} | Due {x.DueDate:yyyy-MM-dd} | Outstanding {x.OutstandingAmount.ToString("N2", CultureInfo.InvariantCulture)}"
            })
            .ToList();
    }
}


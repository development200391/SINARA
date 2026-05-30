
using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("customers")]
    public async Task<IActionResult> Customers(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        string? taxId = null,
        string? contactPerson = null,
        decimal? creditLimitFrom = null,
        decimal? creditLimitTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "taxid", "contactperson", "creditlimit", "paymenttermsdays", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var normalizedTaxId = NormalizeText(taxId);
        var normalizedContactPerson = NormalizeText(contactPerson);
        var (normalizedCreditLimitFrom, normalizedCreditLimitTo) = NormalizeDecimalRange(creditLimitFrom, creditLimitTo);
        var (normalizedPaymentTermsFrom, normalizedPaymentTermsTo) = NormalizeIntRange(paymentTermsFrom, paymentTermsTo);

        var items = await financeApiClient.GetCustomersAsync(accessToken, new CustomerPagedRequest
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
            CreditLimitFrom = normalizedCreditLimitFrom,
            CreditLimitTo = normalizedCreditLimitTo,
            PaymentTermsFrom = normalizedPaymentTermsFrom,
            PaymentTermsTo = normalizedPaymentTermsTo,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Customers";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers";

        return View("Customers/Index", new FinanceCustomersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TaxIdFilter = normalizedTaxId,
            ContactPersonFilter = normalizedContactPerson,
            CreditLimitFromFilter = normalizedCreditLimitFrom,
            CreditLimitToFilter = normalizedCreditLimitTo,
            PaymentTermsFromFilter = normalizedPaymentTermsFrom,
            PaymentTermsToFilter = normalizedPaymentTermsTo,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<CustomerDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("customers/create")]
    public async Task<IActionResult> CreateCustomer(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceCustomerEditViewModel();
        await PopulateCustomerFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Customer";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Create";

        return View("Customers/Create", model);
    }

    [HttpPost("customers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomer(FinanceCustomerEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeCustomerForm(model);
        await PopulateCustomerFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Customer";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Create";
            return View("Customers/Create", model);
        }

        var created = await financeApiClient.CreateCustomerAsync(accessToken, MapCustomerRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create customer.");
            ViewData["Title"] = "Create Customer";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Create";
            return View("Customers/Create", model);
        }

        TempData["SuccessMessage"] = "Customer created.";
        return RedirectToAction(nameof(Customers));
    }

    [HttpGet("customers/edit/{id:int}")]
    public async Task<IActionResult> EditCustomer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var customer = await financeApiClient.GetCustomerByIdAsync(accessToken, id, ct);
        if (customer is null)
        {
            return NotFound();
        }

        var model = new FinanceCustomerEditViewModel
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            TaxId = customer.TaxId,
            Address = customer.Address,
            Phone = customer.Phone,
            Email = customer.Email,
            ContactPerson = customer.ContactPerson,
            CreditLimit = customer.CreditLimit,
            PaymentTermsDays = customer.PaymentTermsDays,
            DefaultAccountId = customer.DefaultAccountId,
            DefaultTaxCodeId = customer.DefaultTaxCodeId,
            IsActive = customer.IsActive
        };

        await PopulateCustomerFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Customer";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Edit";

        return View("Customers/Edit", model);
    }

    [HttpPost("customers/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomer(int id, FinanceCustomerEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeCustomerForm(model);
        await PopulateCustomerFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Customer";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Edit";
            return View("Customers/Edit", model);
        }

        var updated = await financeApiClient.UpdateCustomerAsync(accessToken, id, MapCustomerRequest(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update customer.");
            ViewData["Title"] = "Edit Customer";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / Customers / Edit";
            return View("Customers/Edit", model);
        }

        TempData["SuccessMessage"] = "Customer updated.";
        return RedirectToAction(nameof(Customers));
    }

    [HttpPost("customers/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCustomer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteCustomerAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Customer deleted." : "Failed to delete customer.";
        return RedirectToAction(nameof(Customers));
    }

    [HttpGet("ar/invoices")]
    public async Task<IActionResult> ArInvoices(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "invoicedate",
        string? sortDirection = "desc",
        string? invoiceNo = null,
        int? customerId = null,
        int? periodId = null,
        DateOnly? invoiceDateFrom = null,
        DateOnly? invoiceDateTo = null,
        DateOnly? dueDateFrom = null,
        DateOnly? dueDateTo = null,
        FinanceArInvoiceStatus? status = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "invoicedate", "invoiceno", "customername", "invoicedate", "duedate", "totalamount", "outstandingamount", "status", "sentat", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedInvoiceNo = NormalizeText(invoiceNo);
        var (normalizedInvoiceDateFrom, normalizedInvoiceDateTo) = NormalizeDateRange(invoiceDateFrom, invoiceDateTo);
        var (normalizedDueDateFrom, normalizedDueDateTo) = NormalizeDateRange(dueDateFrom, dueDateTo);
        var (normalizedOutstandingFrom, normalizedOutstandingTo) = NormalizeDecimalRange(outstandingFrom, outstandingTo);

        var itemsTask = financeApiClient.GetArInvoicesAsync(accessToken, new ArInvoicePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            InvoiceNo = normalizedInvoiceNo,
            CustomerId = customerId,
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

        var customerOptionsTask = LoadCustomerOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, customerOptionsTask, periodOptionsTask);

        ViewData["Title"] = "AR Invoices";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices";

        return View("ArInvoices/Index", new FinanceArInvoicesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            InvoiceNoFilter = normalizedInvoiceNo,
            CustomerIdFilter = customerId,
            PeriodIdFilter = periodId,
            InvoiceDateFromFilter = normalizedInvoiceDateFrom,
            InvoiceDateToFilter = normalizedInvoiceDateTo,
            DueDateFromFilter = normalizedDueDateFrom,
            DueDateToFilter = normalizedDueDateTo,
            StatusFilter = status,
            OutstandingFromFilter = normalizedOutstandingFrom,
            OutstandingToFilter = normalizedOutstandingTo,
            IsOverdueFilter = isOverdue,
            CustomerOptions = await customerOptionsTask,
            PeriodOptions = await periodOptionsTask,
            Items = await itemsTask ?? PagedResult<ArInvoiceDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("ar/invoices/create")]
    public async Task<IActionResult> CreateArInvoice(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceArInvoiceEditViewModel();
        await PopulateArInvoiceFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create AR Invoice";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Create";

        return View("ArInvoices/Create", model);
    }

    [HttpPost("ar/invoices/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArInvoice(FinanceArInvoiceEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeArInvoiceForm(model);
        await PopulateArInvoiceFormOptionsAsync(accessToken, model, ct);
        ValidateArInvoiceForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create AR Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Create";
            return View("ArInvoices/Create", model);
        }

        var created = await financeApiClient.CreateArInvoiceAsync(accessToken, MapArInvoiceRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create AR invoice.");
            ViewData["Title"] = "Create AR Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Create";
            return View("ArInvoices/Create", model);
        }

        TempData["SuccessMessage"] = "AR invoice created.";
        return RedirectToAction(nameof(ArInvoices));
    }

    [HttpGet("ar/invoices/edit/{id:int}")]
    public async Task<IActionResult> EditArInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var invoice = await financeApiClient.GetArInvoiceByIdAsync(accessToken, id, ct);
        if (invoice is null)
        {
            return NotFound();
        }

        var model = new FinanceArInvoiceEditViewModel
        {
            Id = invoice.Id,
            InvoiceNo = invoice.InvoiceNo,
            CustomerId = invoice.CustomerId,
            PeriodId = invoice.PeriodId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Description = invoice.Description,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Status = invoice.Status,
            SentAt = invoice.SentAt,
            SentByName = invoice.SentByName,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            ReceivedAmount = invoice.ReceivedAmount,
            OutstandingAmount = invoice.OutstandingAmount,
            IsOverdue = invoice.IsOverdue,
            Lines = invoice.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new FinanceArInvoiceLineEditViewModel
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

        await PopulateArInvoiceFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit AR Invoice";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Edit";

        return View("ArInvoices/Edit", model);
    }

    [HttpPost("ar/invoices/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditArInvoice(int id, FinanceArInvoiceEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var current = await financeApiClient.GetArInvoiceByIdAsync(accessToken, id, ct);
        if (current is null)
        {
            return NotFound();
        }

        if (current.Status != FinanceArInvoiceStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft AR invoice can be edited.";
            return RedirectToAction(nameof(ArInvoices));
        }

        model.Id = id;
        model.InvoiceNo = current.InvoiceNo;
        model.Status = current.Status;

        NormalizeArInvoiceForm(model);
        await PopulateArInvoiceFormOptionsAsync(accessToken, model, ct);
        ValidateArInvoiceForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit AR Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Edit";
            return View("ArInvoices/Edit", model);
        }

        var updated = await financeApiClient.UpdateArInvoiceAsync(accessToken, id, MapArInvoiceRequest(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update AR invoice.");
            ViewData["Title"] = "Edit AR Invoice";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Invoices / Edit";
            return View("ArInvoices/Edit", model);
        }

        TempData["SuccessMessage"] = "AR invoice updated.";
        return RedirectToAction(nameof(ArInvoices));
    }

    [HttpPost("ar/invoices/send/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendArInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var sent = await financeApiClient.SendArInvoiceAsync(accessToken, id, ct);
        TempData[sent is null ? "ErrorMessage" : "SuccessMessage"] = sent is null
            ? "Failed to send AR invoice."
            : "AR invoice sent.";

        return RedirectToAction(nameof(ArInvoices));
    }

    [HttpPost("ar/invoices/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteArInvoice(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteArInvoiceAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "AR invoice deleted." : "Failed to delete AR invoice.";

        return RedirectToAction(nameof(ArInvoices));
    }

    [HttpGet("ar/receipts")]
    public async Task<IActionResult> ArReceipts(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "receiptdate",
        string? sortDirection = "desc",
        string? receiptNo = null,
        int? customerId = null,
        DateOnly? receiptDateFrom = null,
        DateOnly? receiptDateTo = null,
        FinanceArReceiptMethod? paymentMethod = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "receiptdate", "receiptno", "customername", "receiptdate", "amount", "paymentmethod", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedReceiptNo = NormalizeText(receiptNo);
        var (normalizedReceiptDateFrom, normalizedReceiptDateTo) = NormalizeDateRange(receiptDateFrom, receiptDateTo);
        var (normalizedAmountFrom, normalizedAmountTo) = NormalizeDecimalRange(amountFrom, amountTo);

        var itemsTask = financeApiClient.GetArReceiptsAsync(accessToken, new ArReceiptPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ReceiptNo = normalizedReceiptNo,
            CustomerId = customerId,
            ReceiptDateFrom = normalizedReceiptDateFrom,
            ReceiptDateTo = normalizedReceiptDateTo,
            PaymentMethod = paymentMethod,
            AmountFrom = normalizedAmountFrom,
            AmountTo = normalizedAmountTo
        }, ct);

        var customerOptionsTask = LoadCustomerOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, customerOptionsTask);

        ViewData["Title"] = "AR Receipts";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Receipts";

        return View("ArReceipts/Index", new FinanceArReceiptsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ReceiptNoFilter = normalizedReceiptNo,
            CustomerIdFilter = customerId,
            ReceiptDateFromFilter = normalizedReceiptDateFrom,
            ReceiptDateToFilter = normalizedReceiptDateTo,
            PaymentMethodFilter = paymentMethod,
            AmountFromFilter = normalizedAmountFrom,
            AmountToFilter = normalizedAmountTo,
            CustomerOptions = await customerOptionsTask,
            Items = await itemsTask ?? PagedResult<ArReceiptDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("ar/receipts/create")]
    public async Task<IActionResult> CreateArReceipt(int? customerId = null, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceArReceiptCreateViewModel
        {
            CustomerId = customerId is > 0 ? customerId.Value : 0
        };

        await PopulateArReceiptFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create AR Receipt";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Receipts / Create";

        return View("ArReceipts/Create", model);
    }

    [HttpPost("ar/receipts/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArReceipt(FinanceArReceiptCreateViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeArReceiptForm(model);
        await PopulateArReceiptFormOptionsAsync(accessToken, model, ct);
        ValidateArReceiptForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create AR Receipt";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Receipts / Create";
            return View("ArReceipts/Create", model);
        }

        var created = await financeApiClient.CreateArReceiptAsync(accessToken, MapArReceiptRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create AR receipt.");
            ViewData["Title"] = "Create AR Receipt";
            ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Receipts / Create";
            return View("ArReceipts/Create", model);
        }

        TempData["SuccessMessage"] = "AR receipt created.";
        return RedirectToAction(nameof(ArReceipts));
    }

    [HttpGet("ar/aging")]
    public async Task<IActionResult> ArAging(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "totaloutstanding",
        string? sortDirection = "desc",
        int? customerId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "totaloutstanding", "customercode", "customername", "currentamount", "bucket1to30", "bucket31to60", "bucket61to90", "bucketover90", "totaloutstanding");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedAsOfDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var (normalizedOutstandingMin, normalizedOutstandingMax) = NormalizeDecimalRange(outstandingMin, outstandingMax);

        var itemsTask = financeApiClient.GetArAgingAsync(accessToken, new ArAgingPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CustomerId = customerId,
            AsOfDate = normalizedAsOfDate,
            OutstandingMin = normalizedOutstandingMin,
            OutstandingMax = normalizedOutstandingMax
        }, ct);

        var customerOptionsTask = LoadCustomerOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, customerOptionsTask);

        ViewData["Title"] = "AR Aging";
        ViewData["Breadcrumb"] = "Finance / Accounts Receivable / AR Aging";

        return View("ArAging/Index", new FinanceArAgingIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CustomerIdFilter = customerId,
            AsOfDateFilter = normalizedAsOfDate,
            OutstandingMinFilter = normalizedOutstandingMin,
            OutstandingMaxFilter = normalizedOutstandingMax,
            CustomerOptions = await customerOptionsTask,
            Items = await itemsTask ?? PagedResult<ArAgingRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private async Task PopulateCustomerFormOptionsAsync(string accessToken, FinanceCustomerEditViewModel model, CancellationToken ct)
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

    private static void NormalizeCustomerForm(FinanceCustomerEditViewModel model)
    {
        model.Code = string.IsNullOrWhiteSpace(model.Code) ? string.Empty : model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.TaxId = NormalizeText(model.TaxId);
        model.Address = NormalizeText(model.Address);
        model.Phone = NormalizeText(model.Phone);
        model.Email = NormalizeText(model.Email);
        model.ContactPerson = NormalizeText(model.ContactPerson);

        model.DefaultAccountId = model.DefaultAccountId is > 0 ? model.DefaultAccountId : null;
        model.DefaultTaxCodeId = model.DefaultTaxCodeId is > 0 ? model.DefaultTaxCodeId : null;
    }

    private static CustomerDto MapCustomerRequest(FinanceCustomerEditViewModel model)
    {
        return new CustomerDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            TaxId = model.TaxId,
            Address = model.Address,
            Phone = model.Phone,
            Email = model.Email,
            ContactPerson = model.ContactPerson,
            CreditLimit = model.CreditLimit,
            PaymentTermsDays = model.PaymentTermsDays,
            DefaultAccountId = model.DefaultAccountId,
            DefaultTaxCodeId = model.DefaultTaxCodeId,
            IsActive = model.IsActive
        };
    }

    private async Task PopulateArInvoiceFormOptionsAsync(string accessToken, FinanceArInvoiceEditViewModel model, CancellationToken ct)
    {
        var customerOptionsTask = LoadCustomerOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        var taxCodeOptionsTask = LoadTaxCodeOptionsAsync(accessToken, ct);

        await Task.WhenAll(customerOptionsTask, periodOptionsTask, currencyOptionsTask, accountOptionsTask, costCenterOptionsTask, taxCodeOptionsTask);

        model.CustomerOptions = await customerOptionsTask;
        model.PeriodOptions = await periodOptionsTask;
        model.CurrencyOptions = await currencyOptionsTask;
        model.AccountOptions = await accountOptionsTask;
        model.CostCenterOptions = await costCenterOptionsTask;
        model.TaxCodeOptions = await taxCodeOptionsTask;

        if (model.CustomerId <= 0 || model.CustomerOptions.All(x => x.Id != model.CustomerId))
        {
            model.CustomerId = model.CustomerOptions.FirstOrDefault()?.Id ?? 0;
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
            model.Lines = [new FinanceArInvoiceLineEditViewModel()];
        }
    }

    private static void NormalizeArInvoiceForm(FinanceArInvoiceEditViewModel model)
    {
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
            .Select(x => new FinanceArInvoiceLineEditViewModel
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
            model.Lines = [new FinanceArInvoiceLineEditViewModel()];
        }
    }

    private void ValidateArInvoiceForm(FinanceArInvoiceEditViewModel model)
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

    private static ArInvoiceDto MapArInvoiceRequest(FinanceArInvoiceEditViewModel model)
    {
        var lines = model.Lines
            .Select((line, index) => new ArInvoiceLineDto
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

        return new ArInvoiceDto
        {
            Id = model.Id ?? 0,
            InvoiceNo = model.InvoiceNo,
            CustomerId = model.CustomerId,
            PeriodId = model.PeriodId,
            InvoiceDate = model.InvoiceDate,
            DueDate = model.DueDate,
            Description = model.Description,
            CurrencyCode = model.CurrencyCode,
            ExchangeRate = model.ExchangeRate,
            Lines = lines
        };
    }

    private async Task PopulateArReceiptFormOptionsAsync(string accessToken, FinanceArReceiptCreateViewModel model, CancellationToken ct)
    {
        var customerOptionsTask = LoadCustomerOptionsAsync(accessToken, ct);
        var bankAccountOptionsTask = LoadBankAccountOptionsAsync(accessToken, ct);
        var outstandingOptionsTask = LoadOutstandingArInvoiceOptionsAsync(accessToken, ct);

        await Task.WhenAll(customerOptionsTask, bankAccountOptionsTask, outstandingOptionsTask);

        var customerOptions = await customerOptionsTask;
        var bankAccountOptions = await bankAccountOptionsTask;
        var outstandingOptions = await outstandingOptionsTask;

        if (model.CustomerId <= 0 || customerOptions.All(x => x.Id != model.CustomerId))
        {
            model.CustomerId = customerOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (model.BankAccountId <= 0 || bankAccountOptions.All(x => x.Id != model.BankAccountId))
        {
            model.BankAccountId = bankAccountOptions.FirstOrDefault()?.Id ?? 0;
        }

        var validInvoiceMap = outstandingOptions.ToDictionary(x => x.InvoiceId, x => x);

        model.Applications = model.Applications
            .Where(x => x.InvoiceId > 0 && x.AppliedAmount > 0)
            .Where(x => validInvoiceMap.TryGetValue(x.InvoiceId, out var option) && option.CustomerId == model.CustomerId)
            .GroupBy(x => x.InvoiceId)
            .Select(x => new FinanceArReceiptApplicationEditViewModel
            {
                InvoiceId = x.Key,
                AppliedAmount = decimal.Round(x.Sum(y => y.AppliedAmount), 4, MidpointRounding.AwayFromZero)
            })
            .ToList();

        model.CustomerOptions = customerOptions;
        model.BankAccountOptions = bankAccountOptions;
        model.OutstandingInvoiceOptions = outstandingOptions;
    }

    private static void NormalizeArReceiptForm(FinanceArReceiptCreateViewModel model)
    {
        model.ReferenceNo = NormalizeText(model.ReferenceNo);
        model.Notes = NormalizeText(model.Notes);

        var normalizedApplications = model.Applications
            .Where(x => x.InvoiceId > 0 || x.AppliedAmount > 0)
            .Select(x => new FinanceArReceiptApplicationEditViewModel
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

    private void ValidateArReceiptForm(FinanceArReceiptCreateViewModel model)
    {
        if (model.CustomerId <= 0)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Customer is required.");
        }

        if (model.BankAccountId <= 0)
        {
            ModelState.AddModelError(nameof(model.BankAccountId), "Bank account is required.");
        }

        if (model.Applications.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "At least one receipt application is required.");
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

    private static ArReceiptDto MapArReceiptRequest(FinanceArReceiptCreateViewModel model)
    {
        return new ArReceiptDto
        {
            CustomerId = model.CustomerId,
            ReceiptDate = model.ReceiptDate,
            Amount = decimal.Round(model.Amount, 4, MidpointRounding.AwayFromZero),
            PaymentMethod = model.PaymentMethod,
            BankAccountId = model.BankAccountId,
            ReferenceNo = model.ReferenceNo,
            Notes = model.Notes,
            Applications = model.Applications
                .Select(x => new ArReceiptApplicationDto
                {
                    InvoiceId = x.InvoiceId,
                    AppliedAmount = decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero)
                })
                .ToList()
        };
    }

    private async Task<IReadOnlyList<FinanceIdOptionViewModel>> LoadCustomerOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetCustomersAsync(accessToken, new CustomerPagedRequest
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



    private async Task<IReadOnlyList<FinanceArInvoiceOutstandingOptionViewModel>> LoadOutstandingArInvoiceOptionsAsync(string accessToken, CancellationToken ct)
    {
        var approvedTask = financeApiClient.GetArInvoicesAsync(accessToken, new ArInvoicePagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "duedate",
            SortDirection = "asc",
            Status = FinanceArInvoiceStatus.Sent,
            OutstandingFrom = 0.0001m
        }, ct);

        var partiallyPaidTask = financeApiClient.GetArInvoicesAsync(accessToken, new ArInvoicePagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "duedate",
            SortDirection = "asc",
            Status = FinanceArInvoiceStatus.PartiallyPaid,
            OutstandingFrom = 0.0001m
        }, ct);

        await Task.WhenAll(approvedTask, partiallyPaidTask);

        var rows = new List<ArInvoiceDto>();
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
            .Select(x => new FinanceArInvoiceOutstandingOptionViewModel
            {
                CustomerId = x.CustomerId,
                InvoiceId = x.Id,
                InvoiceNo = x.InvoiceNo,
                InvoiceDate = x.InvoiceDate,
                DueDate = x.DueDate,
                OutstandingAmount = x.OutstandingAmount,
                Label = $"{x.InvoiceNo} | {x.CustomerCode} - {x.CustomerName} | Due {x.DueDate:yyyy-MM-dd} | Outstanding {x.OutstandingAmount.ToString("N2", CultureInfo.InvariantCulture)}"
            })
            .ToList();
    }
}














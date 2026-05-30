using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("journals")]
    public async Task<IActionResult> Journals(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "date",
        string? sortDirection = "desc",
        string? journalNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        FinanceJournalSource? source = null,
        FinanceJournalStatus? status = null,
        int? periodId = null,
        string? sourceRefType = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "date", "journalno", "date", "periodname", "source", "status", "postedat", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedJournalNo = NormalizeText(journalNo);
        var normalizedSourceRefType = NormalizeText(sourceRefType);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = financeApiClient.GetJournalsAsync(accessToken, new JournalPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            JournalNo = normalizedJournalNo,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            Source = source,
            Status = status,
            PeriodId = periodId,
            SourceRefType = normalizedSourceRefType
        }, ct);

        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, periodOptionsTask);

        ViewData["Title"] = "Journals";
        ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Journals";

        return View("Journals/Index", new FinanceJournalsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            JournalNoFilter = normalizedJournalNo,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            SourceFilter = source,
            StatusFilter = status,
            PeriodIdFilter = periodId,
            SourceRefTypeFilter = normalizedSourceRefType,
            PeriodOptions = await periodOptionsTask,
            Items = await itemsTask ?? PagedResult<JournalEntryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("journals/create")]
    public async Task<IActionResult> CreateJournal(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceJournalEditViewModel();
        await PopulateJournalFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Journal";
        ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Create";

        return View("Journals/Create", model);
    }

    [HttpPost("journals/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJournal(FinanceJournalEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeJournalForm(model);
        await PopulateJournalFormOptionsAsync(accessToken, model, ct);
        ValidateJournalForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Journal";
            ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Create";
            return View("Journals/Create", model);
        }

        var created = await financeApiClient.CreateJournalAsync(accessToken, MapJournalRequest(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create journal.");
            ViewData["Title"] = "Create Journal";
            ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Create";
            return View("Journals/Create", model);
        }

        TempData["SuccessMessage"] = "Journal created.";
        return RedirectToAction(nameof(Journals));
    }

    [HttpGet("journals/edit/{id:int}")]
    public async Task<IActionResult> EditJournal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var journal = await financeApiClient.GetJournalByIdAsync(accessToken, id, ct);
        if (journal is null)
        {
            return NotFound();
        }

        var model = new FinanceJournalEditViewModel
        {
            Id = journal.Id,
            JournalNo = journal.JournalNo,
            PeriodId = journal.PeriodId,
            Date = journal.Date,
            Description = journal.Description,
            Source = journal.Source,
            SourceRefId = journal.SourceRefId,
            SourceRefType = journal.SourceRefType,
            Status = journal.Status,
            PostedAt = journal.PostedAt,
            PostedByName = journal.PostedByName,
            ReversedJournalId = journal.ReversedJournalId,
            CurrencyCode = journal.CurrencyCode,
            ExchangeRate = journal.ExchangeRate,
            Lines = journal.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new FinanceJournalLineEditViewModel
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    CostCenterId = x.CostCenterId,
                    Description = x.Description,
                    Debit = x.Debit,
                    Credit = x.Credit
                })
                .ToList()
        };

        await PopulateJournalFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Journal";
        ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Edit";

        return View("Journals/Edit", model);
    }

    [HttpPost("journals/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJournal(int id, FinanceJournalEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var current = await financeApiClient.GetJournalByIdAsync(accessToken, id, ct);
        if (current is null)
        {
            return NotFound();
        }

        if (current.Status != FinanceJournalStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft journal can be edited.";
            return RedirectToAction(nameof(Journals));
        }

        model.Id = id;
        model.JournalNo = current.JournalNo;
        model.Status = current.Status;

        NormalizeJournalForm(model);
        await PopulateJournalFormOptionsAsync(accessToken, model, ct);
        ValidateJournalForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Journal";
            ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Edit";
            return View("Journals/Edit", model);
        }

        var updated = await financeApiClient.UpdateJournalAsync(accessToken, id, MapJournalRequest(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update journal.");
            ViewData["Title"] = "Edit Journal";
            ViewData["Breadcrumb"] = "Finance / Journal & Ledger / Edit";
            return View("Journals/Edit", model);
        }

        TempData["SuccessMessage"] = "Journal updated.";
        return RedirectToAction(nameof(Journals));
    }

    [HttpPost("journals/post/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostJournal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await financeApiClient.PostJournalAsync(accessToken, id, ct);
        TempData[result is null ? "ErrorMessage" : "SuccessMessage"] = result is null
            ? "Failed to post journal. Ensure journal is balanced."
            : "Journal posted.";

        return RedirectToAction(nameof(Journals));
    }

    [HttpPost("journals/reverse/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReverseJournal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await financeApiClient.ReverseJournalAsync(accessToken, id, ct);
        TempData[result is null ? "ErrorMessage" : "SuccessMessage"] = result is null
            ? "Failed to reverse journal."
            : "Journal reversed.";

        return RedirectToAction(nameof(Journals));
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> Ledger(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "accountcode",
        string? sortDirection = "asc",
        int? accountId = null,
        int? periodId = null,
        int? costCenterId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "accountcode", "accountcode", "accountname", "date", "journalno", "periodname", "debit", "credit", "balance");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = financeApiClient.GetLedgerAsync(accessToken, new LedgerPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AccountId = accountId,
            PeriodId = periodId,
            CostCenterId = costCenterId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, accountOptionsTask, periodOptionsTask, costCenterOptionsTask);

        ViewData["Title"] = "General Ledger";
        ViewData["Breadcrumb"] = "Finance / Journal & Ledger / General Ledger";

        return View("Ledger/Index", new FinanceLedgerIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AccountIdFilter = accountId,
            PeriodIdFilter = periodId,
            CostCenterIdFilter = costCenterId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            AccountOptions = await accountOptionsTask,
            PeriodOptions = await periodOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            Items = await itemsTask ?? PagedResult<LedgerEntryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private async Task PopulateJournalFormOptionsAsync(string accessToken, FinanceJournalEditViewModel model, CancellationToken ct)
    {
        var periodOptionsTask = LoadPeriodOptionsAsync(accessToken, ct);
        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);
        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = LoadCostCenterOptionsAsync(accessToken, ct);

        await Task.WhenAll(periodOptionsTask, currencyOptionsTask, accountOptionsTask, costCenterOptionsTask);

        model.PeriodOptions = await periodOptionsTask;
        model.CurrencyOptions = await currencyOptionsTask;
        model.AccountOptions = await accountOptionsTask;
        model.CostCenterOptions = await costCenterOptionsTask;

        if (model.PeriodId <= 0)
        {
            model.PeriodId = model.PeriodOptions.FirstOrDefault()?.Id ?? 0;
        }

        if (string.IsNullOrWhiteSpace(model.CurrencyCode))
        {
            model.CurrencyCode = model.CurrencyOptions.FirstOrDefault()?.Code ?? "IDR";
        }

        if (model.ExchangeRate <= 0)
        {
            model.ExchangeRate = 1m;
        }

        if (model.Lines.Count == 0)
        {
            model.Lines =
            [
                new FinanceJournalLineEditViewModel(),
                new FinanceJournalLineEditViewModel()
            ];
        }
    }

    private static void NormalizeJournalForm(FinanceJournalEditViewModel model)
    {
        model.Description = model.Description?.Trim() ?? string.Empty;
        model.SourceRefType = NormalizeText(model.SourceRefType);
        model.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode)
            ? "IDR"
            : model.CurrencyCode.Trim().ToUpperInvariant();

        var filteredLines = model.Lines
            .Where(x => x.AccountId > 0 || x.Debit > 0 || x.Credit > 0 || !string.IsNullOrWhiteSpace(x.Description) || x.CostCenterId.HasValue)
            .Select(x => new FinanceJournalLineEditViewModel
            {
                Id = x.Id,
                AccountId = x.AccountId,
                CostCenterId = x.CostCenterId is > 0 ? x.CostCenterId : null,
                Description = NormalizeText(x.Description),
                Debit = x.Debit,
                Credit = x.Credit
            })
            .ToList();

        model.Lines = filteredLines;
    }

    private void ValidateJournalForm(FinanceJournalEditViewModel model)
    {
        if (model.Lines.Count < 2)
        {
            ModelState.AddModelError(string.Empty, "At least two journal lines are required.");
            return;
        }

        for (var index = 0; index < model.Lines.Count; index++)
        {
            var line = model.Lines[index];

            if (line.AccountId <= 0)
            {
                ModelState.AddModelError($"Lines[{index}].AccountId", $"Account is required at line {index + 1}.");
            }

            if (line.Debit < 0 || line.Credit < 0)
            {
                ModelState.AddModelError(string.Empty, $"Debit/Credit cannot be negative at line {index + 1}.");
            }

            if (line.Debit > 0 && line.Credit > 0)
            {
                ModelState.AddModelError(string.Empty, $"Line {index + 1} can only have debit or credit.");
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                ModelState.AddModelError(string.Empty, $"Line {index + 1} must have debit or credit value.");
            }
        }
    }

    private static JournalEntryDto MapJournalRequest(FinanceJournalEditViewModel model)
    {
        return new JournalEntryDto
        {
            Id = model.Id ?? 0,
            PeriodId = model.PeriodId,
            Date = model.Date,
            Description = model.Description,
            Source = model.Source,
            SourceRefId = model.SourceRefId,
            SourceRefType = model.SourceRefType,
            CurrencyCode = model.CurrencyCode,
            ExchangeRate = model.ExchangeRate,
            Lines = model.Lines
                .Select((line, index) => new JournalLineDto
                {
                    Id = line.Id ?? 0,
                    LineNo = index + 1,
                    AccountId = line.AccountId,
                    CostCenterId = line.CostCenterId,
                    Description = line.Description,
                    Debit = line.Debit,
                    Credit = line.Credit
                })
                .ToList()
        };
    }
}

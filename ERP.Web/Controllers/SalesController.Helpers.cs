namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
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

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private async Task<IReadOnlyList<string>> LoadCurrencyCodesAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetCurrenciesAsync(accessToken, new CurrencyPagedRequest
        {
            Page = 1,
            PageSize = 200,
            SortBy = "code",
            SortDirection = "asc",
            IsActive = true
        }, ct);

        return result?.Items
            .Select(x => x.Code)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? ["IDR"];
    }

    private async Task PopulatePriceListItemOptionsAsync(string accessToken, SalesPriceListItemEditViewModel model, CancellationToken ct)
    {
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomOptionsTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemOptionsTask, uomOptionsTask);

        model.ItemOptions = (await itemOptionsTask).OrderBy(x => x.Label).ToList();
        model.UomOptions = (await uomOptionsTask).OrderBy(x => x.Label).ToList();
    }

    private async Task PopulateApprovalConfigFormOptionsAsync(string accessToken, SalesApprovalConfigEditViewModel model, CancellationToken ct)
    {
        var roleOptionsTask = configApiClient.GetRolesAsync(accessToken, ct);
        var employeeOptionsTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);

        await Task.WhenAll(roleOptionsTask, employeeOptionsTask);

        model.RoleOptions = (await roleOptionsTask).Where(x => x.IsActive).OrderBy(x => x.Name).ToList();
        model.EmployeeOptions = (await employeeOptionsTask).OrderBy(x => x.Name).ToList();
    }

    private static void NormalizeCustomerCategoryForm(SalesCustomerCategoryEditViewModel model)
    {
        model.Code = string.IsNullOrWhiteSpace(model.Code) ? string.Empty : model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Description = NormalizeText(model.Description);
        model.DefaultPriceListId = model.DefaultPriceListId is > 0 ? model.DefaultPriceListId : null;
    }

    private static void NormalizePriceListForm(SalesPriceListEditViewModel model)
    {
        model.Code = string.IsNullOrWhiteSpace(model.Code) ? string.Empty : model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "IDR" : model.CurrencyCode.Trim().ToUpperInvariant();
        model.Notes = NormalizeText(model.Notes);
    }

    private static void NormalizePriceListItemForm(SalesPriceListItemEditViewModel model)
    {
        if (model.MinQty <= 0)
        {
            model.MinQty = 1m;
        }
    }

    private static void NormalizeApprovalConfigForm(SalesApprovalConfigEditViewModel model)
    {
        model.ApproverRoleId = model.ApproverRoleId is > 0 ? model.ApproverRoleId : null;
        model.ApproverEmployeeId = model.ApproverEmployeeId is > 0 ? model.ApproverEmployeeId : null;
    }

    private static void NormalizeTeamForm(SalesTeamEditViewModel model)
    {
        model.Code = string.IsNullOrWhiteSpace(model.Code) ? string.Empty : model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.MemberEmployeeIds = model.MemberEmployeeIds
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }

    private void ValidateApprovalConfigForm(SalesApprovalConfigEditViewModel model)
    {
        if (model.MaxAmount.HasValue && model.MaxAmount.Value < model.MinAmount)
        {
            ModelState.AddModelError(nameof(model.MaxAmount), "Maximum amount must be greater than or equal to minimum amount.");
        }

        if (!model.ApproverRoleId.HasValue && !model.ApproverEmployeeId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "At least approver role or approver employee must be selected.");
        }
    }

    private static SalesCustomerCategoryDto MapCustomerCategoryRequest(SalesCustomerCategoryEditViewModel model)
    {
        return new SalesCustomerCategoryDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            DefaultPriceListId = model.DefaultPriceListId,
            DefaultPaymentTerms = model.DefaultPaymentTerms,
            DefaultCreditLimit = model.DefaultCreditLimit,
            Description = model.Description,
            IsActive = model.IsActive
        };
    }

    private static SalesPriceListDto MapPriceListRequest(SalesPriceListEditViewModel model)
    {
        return new SalesPriceListDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            Type = model.Type,
            CurrencyCode = model.CurrencyCode,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static SalesPriceListItemDto MapPriceListItemRequest(SalesPriceListItemEditViewModel model)
    {
        return new SalesPriceListItemDto
        {
            Id = model.Id ?? 0,
            PriceListId = model.PriceListId,
            ItemId = model.ItemId,
            UomId = model.UomId,
            MinQty = decimal.Round(model.MinQty, 4, MidpointRounding.AwayFromZero),
            UnitPrice = decimal.Round(model.UnitPrice, 4, MidpointRounding.AwayFromZero),
            DiscountPct = decimal.Round(model.DiscountPct, 2, MidpointRounding.AwayFromZero)
        };
    }

    private static SalesApprovalConfigDto MapApprovalConfigRequest(SalesApprovalConfigEditViewModel model)
    {
        return new SalesApprovalConfigDto
        {
            Id = model.Id ?? 0,
            DocumentType = model.DocumentType,
            Level = model.Level,
            MinAmount = model.MinAmount,
            MaxAmount = model.MaxAmount,
            MaxDiscountPct = model.MaxDiscountPct,
            ApproverRoleId = model.ApproverRoleId,
            ApproverEmployeeId = model.ApproverEmployeeId,
            TimeoutHours = model.TimeoutHours,
            AutoApproveIfTimeout = model.AutoApproveIfTimeout,
            IsActive = model.IsActive
        };
    }

    private static SalesTeamDto MapTeamRequest(SalesTeamEditViewModel model)
    {
        return new SalesTeamDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            TeamLeaderId = model.TeamLeaderId,
            IsActive = model.IsActive,
            MemberEmployeeIds = model.MemberEmployeeIds
        };
    }
}

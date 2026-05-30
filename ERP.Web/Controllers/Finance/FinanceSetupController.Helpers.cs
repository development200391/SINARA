using System.Security.Claims;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    private IActionResult? RequireAccessToken(out string accessToken, bool includeReturnUrl = true)
    {
        accessToken = GetAccessToken() ?? string.Empty;
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

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
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

    private static (decimal? From, decimal? To) NormalizeDecimalRange(decimal? from, decimal? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static void NormalizeAccountForm(FinanceAccountEditViewModel model)
    {
        model.ParentAccountId = model.ParentAccountId is > 0 ? model.ParentAccountId : null;

        if (!model.IsBankAccount)
        {
            model.BankName = null;
            model.BankAccountNo = null;
        }
    }

    private static AccountDto MapAccountDto(FinanceAccountEditViewModel model)
    {
        return new AccountDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            GroupId = model.GroupId,
            Type = model.Type,
            NormalBalance = model.NormalBalance,
            IsHeader = model.IsHeader,
            ParentAccountId = model.ParentAccountId,
            Description = NormalizeText(model.Description),
            IsBankAccount = model.IsBankAccount,
            BankName = NormalizeText(model.BankName),
            BankAccountNo = NormalizeText(model.BankAccountNo),
            CurrencyCode = model.CurrencyCode,
            IsActive = model.IsActive
        };
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}

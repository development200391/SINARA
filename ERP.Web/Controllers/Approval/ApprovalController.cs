using System.Security.Claims;
using ERP.Application.DTOs.Approval;
using ERP.Web.Services;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERP.Web.Controllers;

[Authorize]
[Route("approval")]
public sealed partial class ApprovalController(IApprovalApiClient approvalApiClient) : Controller
{
    private const int DefaultPageSize = 20;

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

    private static string NormalizeSortDirection(string? sortDirection)
        => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

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

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static string ResolveApiErrorMessage<T>(ApiCallResult<T> result, string fallbackMessage)
        => string.IsNullOrWhiteSpace(result.ErrorMessage) ? fallbackMessage : result.ErrorMessage;

    private void AddApiModelError<T>(ApiCallResult<T> result, string fallbackMessage)
        => ModelState.AddModelError(string.Empty, ResolveApiErrorMessage(result, fallbackMessage));

    private async Task PopulateDelegationFormOptionsAsync(string accessToken, ApprovalDelegationEditViewModel model, CancellationToken ct)
    {
        var userOptionsTask = approvalApiClient.GetApproverOptionsAsync(accessToken, ct);
        var templateOptionsTask = approvalApiClient.GetTemplateOptionsAsync(accessToken, ct);

        await Task.WhenAll(userOptionsTask, templateOptionsTask);

        model.UserOptions = await userOptionsTask;
        model.TemplateOptions = await templateOptionsTask;
    }

    private static void ValidateTemplateForm(ApprovalTemplateEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.MinAmount.HasValue && model.MaxAmount.HasValue && model.MinAmount.Value > model.MaxAmount.Value)
        {
            modelState.AddModelError(nameof(model.MaxAmount), "Maximum amount must be greater than or equal to minimum amount.");
        }

        if (model.AutoApproveBelow.HasValue && model.AutoApproveBelow.Value < 0)
        {
            modelState.AddModelError(nameof(model.AutoApproveBelow), "Auto approve amount cannot be negative.");
        }
    }

    private static void ValidateLevelForm(ApprovalLevelEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.ApproverType == ERP.Domain.Enums.Approval.ApprovalApproverType.Role && !model.ApproverRoleId.HasValue)
        {
            modelState.AddModelError(nameof(model.ApproverRoleId), "Approver role is required for role-based level.");
        }

        if (model.ApproverType == ERP.Domain.Enums.Approval.ApprovalApproverType.Position && !model.ApproverPositionId.HasValue)
        {
            modelState.AddModelError(nameof(model.ApproverPositionId), "Approver position is required for position-based level.");
        }

        if (model.ApproverType == ERP.Domain.Enums.Approval.ApprovalApproverType.SpecificUser && !model.ApproverUserId.HasValue)
        {
            modelState.AddModelError(nameof(model.ApproverUserId), "Approver user is required for specific-user level.");
        }
    }

    private static void ValidateDelegationForm(ApprovalDelegationEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.StartDate > model.EndDate)
        {
            modelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        }

        if (model.DelegatorUserId == model.DelegateUserId)
        {
            modelState.AddModelError(nameof(model.DelegateUserId), "Delegate must be different from delegator.");
        }
    }

    private static ApprovalTemplateDto MapTemplateDto(ApprovalTemplateEditViewModel model)
    {
        return new ApprovalTemplateDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            Module = model.Module,
            ReferenceType = model.ReferenceType,
            ApprovalType = model.ApprovalType,
            MinAmount = model.MinAmount,
            MaxAmount = model.MaxAmount,
            AutoApproveBelow = model.AutoApproveBelow,
            SlaHours = model.SlaHours,
            AllowDelegation = model.AllowDelegation,
            RequireCommentOnReject = model.RequireCommentOnReject,
            IsActive = model.IsActive
        };
    }

    private static ApprovalLevelDto MapLevelDto(ApprovalLevelEditViewModel model)
    {
        return new ApprovalLevelDto
        {
            Id = model.Id ?? 0,
            TemplateId = model.TemplateId,
            LevelOrder = model.LevelOrder,
            LevelName = model.LevelName,
            ApproverType = model.ApproverType,
            ApproverRoleId = model.ApproverRoleId,
            ApproverPositionId = model.ApproverPositionId,
            ApproverUserId = model.ApproverUserId,
            MinApproversRequired = model.MinApproversRequired,
            EscalationHours = model.EscalationHours,
            EscalateToLevelId = model.EscalateToLevelId,
            IsActive = model.IsActive
        };
    }

    private static ApprovalDelegationDto MapDelegationDto(ApprovalDelegationEditViewModel model)
    {
        return new ApprovalDelegationDto
        {
            Id = model.Id ?? 0,
            DelegatorUserId = model.DelegatorUserId,
            DelegateUserId = model.DelegateUserId,
            TemplateId = model.TemplateId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = NormalizeText(model.Reason),
            IsActive = model.IsActive
        };
    }
}

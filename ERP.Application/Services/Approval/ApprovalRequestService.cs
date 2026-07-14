using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Entities.Approval;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums;
using ERP.Domain.Enums.Approval;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Approval;

public sealed class ApprovalRequestService(
    IUnitOfWork unitOfWork,
    IEnumerable<IApprovalCallbackService> callbackServices,
    IApprovalNotificationService notificationService) : IApprovalRequestService
{
    public async Task<ApprovalRequestDto> SubmitAsync(
        string module,
        string referenceType,
        int referenceId,
        string subject,
        decimal? amount,
        int requestedByUserId,
        string? notes,
        CancellationToken ct = default)
    {
        var duplicate = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .AnyAsync(x => x.ReferenceType == referenceType && x.ReferenceId == referenceId &&
                (x.Status == ApprovalRequestStatus.Pending || x.Status == ApprovalRequestStatus.InProgress), ct);
        if (duplicate)
        {
            throw new InvalidOperationException($"An approval request is already in progress for {referenceType} #{referenceId}.");
        }

        var templates = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .Where(x => x.ReferenceType == referenceType && x.IsActive)
            .Include(x => x.Levels)
            .ToListAsync(ct);

        var template = templates
            .Where(x => (!x.MinAmount.HasValue || !amount.HasValue || amount.Value >= x.MinAmount.Value) &&
                        (!x.MaxAmount.HasValue || !amount.HasValue || amount.Value <= x.MaxAmount.Value))
            .OrderBy(x => x.Id)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"No active approval template configured for reference type '{referenceType}'" +
                (amount.HasValue ? $" and amount {amount.Value}." : "."));

        var now = DateTimeOffset.UtcNow;
        var requestNo = await GenerateRequestNoAsync(now, ct);

        var request = new ApprovalRequest
        {
            RequestNo = requestNo,
            TemplateId = template.Id,
            Module = module,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Subject = subject,
            Amount = amount,
            RequestedBy = requestedByUserId,
            RequestedAt = now,
            Status = ApprovalRequestStatus.Pending,
            Notes = notes,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<ApprovalRequest>().AddAsync(request, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await WriteAuditAsync(request.Id, null, requestedByUserId, "CREATED", module, null, ApprovalRequestStatus.Pending, notes, ct);

        if (template.AutoApproveBelow.HasValue && amount.HasValue && amount.Value < template.AutoApproveBelow.Value)
        {
            request.Status = ApprovalRequestStatus.Approved;
            request.FinalActionAt = now;
            unitOfWork.Repository<ApprovalRequest>().Update(request);
            await unitOfWork.SaveChangesAsync(ct);

            await WriteAuditAsync(request.Id, null, requestedByUserId, "AUTO_APPROVED", module, ApprovalRequestStatus.Pending, ApprovalRequestStatus.Approved, "Amount below auto-approve threshold.", ct);
            await DispatchApprovedOrRejectedCallbackAsync(request, approved: true, comment: null, requestedByUserId, ct);

            return await LoadRequestDtoAsync(request.Id, ct);
        }

        var firstLevel = template.Levels.Where(x => x.IsActive).OrderBy(x => x.LevelOrder).FirstOrDefault()
            ?? throw new InvalidOperationException($"Template '{template.Code}' has no active approval levels configured.");

        await ActivateLevelAsync(request, template, firstLevel, ct);

        return await LoadRequestDtoAsync(request.Id, ct);
    }

    public async Task<PagedResult<ApprovalInboxDto>> GetInboxPagedAsync(int userId, ApprovalInboxPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var now = DateTimeOffset.UtcNow;

        var query = unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .Include(x => x.Request!).ThenInclude(r => r.Template)
            .Include(x => x.Request!).ThenInclude(r => r.RequestedByUser)
            .Include(x => x.Level)
            .Where(x => x.ApproverUserId == userId && x.IsActive && x.Action == null);

        if (!string.IsNullOrWhiteSpace(request.RequestNo))
        {
            var no = request.RequestNo.Trim().ToLower();
            query = query.Where(x => x.Request!.RequestNo.ToLower().Contains(no));
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            var module = request.Module.Trim().ToLower();
            query = query.Where(x => x.Request!.Module.ToLower() == module);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            var referenceType = request.ReferenceType.Trim().ToLower();
            query = query.Where(x => x.Request!.ReferenceType.ToLower() == referenceType);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Request!.Status == request.Status.Value);
        }

        if (request.RequestedDateFrom.HasValue)
        {
            var from = new DateTimeOffset(request.RequestedDateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.Request!.RequestedAt >= from);
        }

        if (request.RequestedDateTo.HasValue)
        {
            var to = new DateTimeOffset(request.RequestedDateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.Request!.RequestedAt <= to);
        }

        if (request.IsOverdue.HasValue)
        {
            query = request.IsOverdue.Value ? query.Where(x => x.DueAt < now) : query.Where(x => x.DueAt >= now);
        }

        var total = await query.CountAsync(ct);

        var entities = await query.OrderBy(x => x.DueAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var items = entities.Select(x => new ApprovalInboxDto
        {
            RequestId = x.RequestId,
            StepId = x.Id,
            RequestNo = x.Request?.RequestNo ?? string.Empty,
            Subject = x.Request?.Subject ?? string.Empty,
            Module = x.Request?.Module ?? string.Empty,
            ReferenceType = x.Request?.ReferenceType ?? string.Empty,
            ReferenceId = x.Request?.ReferenceId ?? 0,
            TemplateCode = x.Request?.Template?.Code ?? string.Empty,
            TemplateName = x.Request?.Template?.Name ?? string.Empty,
            LevelOrder = x.LevelOrder,
            LevelName = x.Level?.LevelName ?? string.Empty,
            Amount = x.Request?.Amount,
            RequestedByName = x.Request?.RequestedByUser?.FullName ?? string.Empty,
            RequestedAt = x.Request?.RequestedAt ?? default,
            DueAt = x.DueAt,
            IsOverdue = x.DueAt < now,
            Status = x.Request?.Status ?? ApprovalRequestStatus.Pending
        }).ToList();

        return PagedResult<ApprovalInboxDto>.Create(items, total, page, pageSize);
    }

    public async Task<PagedResult<ApprovalRequestDto>> GetMyRequestsPagedAsync(int userId, ApprovalRequestPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.CurrentLevel)
            .Include(x => x.RequestedByUser)
            .Include(x => x.FinalActionByUser)
            .Where(x => x.RequestedBy == userId);

        if (!string.IsNullOrWhiteSpace(request.RequestNo))
        {
            var no = request.RequestNo.Trim().ToLower();
            query = query.Where(x => x.RequestNo.ToLower().Contains(no));
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            var module = request.Module.Trim().ToLower();
            query = query.Where(x => x.Module.ToLower() == module);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            var referenceType = request.ReferenceType.Trim().ToLower();
            query = query.Where(x => x.ReferenceType.ToLower() == referenceType);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.RequestedDateFrom.HasValue)
        {
            var from = new DateTimeOffset(request.RequestedDateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.RequestedAt >= from);
        }

        if (request.RequestedDateTo.HasValue)
        {
            var to = new DateTimeOffset(request.RequestedDateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.RequestedAt <= to);
        }

        var total = await query.CountAsync(ct);

        var entities = await query.OrderByDescending(x => x.RequestedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = entities.Select(MapRequest).ToList();

        return PagedResult<ApprovalRequestDto>.Create(items, total, page, pageSize);
    }

    public Task<ApprovalRequestDto> ApproveAsync(int requestId, int actorUserId, TakeApprovalActionRequest request, CancellationToken ct = default)
        => ProcessActionAsync(requestId, actorUserId, ApprovalStepAction.Approved, request, ct);

    public Task<ApprovalRequestDto> RejectAsync(int requestId, int actorUserId, TakeApprovalActionRequest request, CancellationToken ct = default)
        => ProcessActionAsync(requestId, actorUserId, ApprovalStepAction.Rejected, request, ct);

    public async Task<ApprovalRequestDto> CancelAsync(int requestId, int actorUserId, string? notes, CancellationToken ct = default)
    {
        var request = await unitOfWork.Repository<ApprovalRequest>().GetByIdAsync(requestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        if (request.RequestedBy != actorUserId)
        {
            throw new UnauthorizedAccessException("Only the requester can cancel this request.");
        }

        if (request.Status != ApprovalRequestStatus.Pending && request.Status != ApprovalRequestStatus.InProgress)
        {
            throw new InvalidOperationException("Only pending or in-progress requests can be cancelled.");
        }

        var hasAnyApproval = await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .AnyAsync(x => x.RequestId == requestId && x.Action == ApprovalStepAction.Approved, ct);
        if (hasAnyApproval)
        {
            throw new InvalidOperationException("This request cannot be cancelled because it already has at least one approval.");
        }

        var activeSteps = await unitOfWork.Repository<ApprovalStep>().Query()
            .Where(x => x.RequestId == requestId && x.IsActive && x.Action == null)
            .ToListAsync(ct);
        foreach (var step in activeSteps)
        {
            step.IsActive = false;
            unitOfWork.Repository<ApprovalStep>().Update(step);
        }

        var oldStatus = request.Status;
        request.Status = ApprovalRequestStatus.Cancelled;
        request.FinalActionAt = DateTimeOffset.UtcNow;
        request.FinalActionBy = actorUserId;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            request.Notes = notes;
        }

        unitOfWork.Repository<ApprovalRequest>().Update(request);
        await unitOfWork.SaveChangesAsync(ct);

        await WriteAuditAsync(requestId, null, actorUserId, "CANCELLED", request.Module, oldStatus, ApprovalRequestStatus.Cancelled, notes, ct);

        var callback = callbackServices.FirstOrDefault(x => string.Equals(x.ReferenceType, request.ReferenceType, StringComparison.OrdinalIgnoreCase));
        if (callback is not null)
        {
            await callback.OnCancelledAsync(request.ReferenceId, actorUserId, ct);
        }

        return await LoadRequestDtoAsync(requestId, ct);
    }

    public async Task<int?> FindActiveRequestIdAsync(string referenceType, int referenceId, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .Where(x => x.ReferenceType == referenceType && x.ReferenceId == referenceId &&
                (x.Status == ApprovalRequestStatus.Pending || x.Status == ApprovalRequestStatus.InProgress))
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyDictionary<int, bool>> GetActionablePermissionsAsync(string referenceType, IReadOnlyCollection<int> referenceIds, int userId, CancellationToken ct = default)
    {
        if (referenceIds.Count == 0)
        {
            return new Dictionary<int, bool>();
        }

        var activeRequests = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .Where(x => x.ReferenceType == referenceType && referenceIds.Contains(x.ReferenceId) &&
                (x.Status == ApprovalRequestStatus.Pending || x.Status == ApprovalRequestStatus.InProgress))
            .Select(x => new { x.Id, x.ReferenceId })
            .ToListAsync(ct);

        var requestIds = activeRequests.Select(x => x.Id).ToList();

        var userStepRequestIds = requestIds.Count == 0
            ? []
            : await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
                .Where(x => requestIds.Contains(x.RequestId) && x.ApproverUserId == userId && x.IsActive && x.Action == null)
                .Select(x => x.RequestId)
                .Distinct()
                .ToListAsync(ct);

        var userStepRequestIdSet = userStepRequestIds.ToHashSet();

        var result = new Dictionary<int, bool>();
        foreach (var referenceId in referenceIds)
        {
            var activeRequest = activeRequests.FirstOrDefault(x => x.ReferenceId == referenceId);
            result[referenceId] = activeRequest is null || userStepRequestIdSet.Contains(activeRequest.Id);
        }

        return result;
    }

    public async Task ProcessEscalationsAndRemindersAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var activeSteps = await unitOfWork.Repository<ApprovalStep>().Query()
            .Include(x => x.Request!).ThenInclude(r => r.Template)
            .Include(x => x.Level)
            .Where(x => x.IsActive && x.Action == null)
            .ToListAsync(ct);

        foreach (var step in activeSteps)
        {
            if (step.Request is null || step.Request.Status is not (ApprovalRequestStatus.Pending or ApprovalRequestStatus.InProgress))
            {
                continue;
            }

            var remaining = step.DueAt - now;

            if (remaining <= TimeSpan.FromHours(4) && remaining > TimeSpan.Zero && step.ReminderCount == 0)
            {
                step.ReminderCount = 1;
                step.NotifiedAt = now;
                unitOfWork.Repository<ApprovalStep>().Update(step);
                await unitOfWork.SaveChangesAsync(ct);

                await notificationService.NotifyAsync(step.RequestId, step.Id, step.ApproverUserId, ApprovalNotificationType.Reminder,
                    $"Reminder: approval due soon - {step.Request.Subject}",
                    $"Request {step.Request.RequestNo} is due by {step.DueAt:yyyy-MM-dd HH:mm} UTC.", ct);
                continue;
            }

            if (remaining <= TimeSpan.FromHours(1) && remaining > TimeSpan.Zero && step.ReminderCount == 1)
            {
                step.ReminderCount = 2;
                step.NotifiedAt = now;
                unitOfWork.Repository<ApprovalStep>().Update(step);
                await unitOfWork.SaveChangesAsync(ct);

                await notificationService.NotifyAsync(step.RequestId, step.Id, step.ApproverUserId, ApprovalNotificationType.Reminder,
                    $"Urgent reminder: approval due within 1 hour - {step.Request.Subject}",
                    $"Request {step.Request.RequestNo} is due by {step.DueAt:yyyy-MM-dd HH:mm} UTC.", ct);
                continue;
            }

            if (remaining > TimeSpan.Zero)
            {
                continue;
            }

            if (step.Level?.EscalateToLevelId is int escalateToLevelId)
            {
                var escalateToLevel = await unitOfWork.Repository<ApprovalLevel>().GetByIdAsync(escalateToLevelId, ct);
                if (escalateToLevel is not null)
                {
                    step.IsActive = false;
                    unitOfWork.Repository<ApprovalStep>().Update(step);

                    var siblingSteps = await unitOfWork.Repository<ApprovalStep>().Query()
                        .Where(x => x.RequestId == step.RequestId && x.LevelId == step.LevelId && x.IsActive && x.Action == null && x.Id != step.Id)
                        .ToListAsync(ct);
                    foreach (var sibling in siblingSteps)
                    {
                        sibling.IsActive = false;
                        unitOfWork.Repository<ApprovalStep>().Update(sibling);
                    }

                    await unitOfWork.SaveChangesAsync(ct);

                    await WriteAuditAsync(step.RequestId, step.Id, step.ApproverUserId, "ESCALATED", step.Request.Module, step.Request.Status, step.Request.Status,
                        $"Escalated from level '{step.Level.LevelName}' to '{escalateToLevel.LevelName}' after SLA breach.", ct);

                    await ActivateLevelAsync(step.Request, step.Request.Template!, escalateToLevel, ct);
                    continue;
                }
            }

            var alreadyAlerted = await unitOfWork.Repository<ApprovalAuditLog>().Query().AsNoTracking()
                .AnyAsync(x => x.RequestId == step.RequestId && x.StepId == step.Id && x.Action == "OVERDUE_ALERT", ct);
            if (alreadyAlerted)
            {
                continue;
            }

            await WriteAuditAsync(step.RequestId, step.Id, step.ApproverUserId, "OVERDUE_ALERT", step.Request.Module, step.Request.Status, step.Request.Status,
                $"Step for level '{step.Level?.LevelName}' is overdue with no escalation target configured.", ct);

            var superAdminUserIds = await unitOfWork.Repository<SysUserRole>().Query().AsNoTracking()
                .Where(x => x.Role.Name == "Super Admin" && x.User.IsActive)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(ct);

            foreach (var adminUserId in superAdminUserIds)
            {
                await notificationService.NotifyAsync(step.RequestId, step.Id, adminUserId, ApprovalNotificationType.Escalated,
                    $"Overdue approval: {step.Request.Subject}",
                    $"Request {step.Request.RequestNo} is overdue at level '{step.Level?.LevelName}' with no escalation target. Please follow up.", ct);
            }
        }
    }

    private async Task<ApprovalRequestDto> ProcessActionAsync(int requestId, int actorUserId, ApprovalStepAction action, TakeApprovalActionRequest actionRequest, CancellationToken ct)
    {
        var request = await unitOfWork.Repository<ApprovalRequest>().Query()
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == requestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        if (request.Status != ApprovalRequestStatus.Pending && request.Status != ApprovalRequestStatus.InProgress)
        {
            throw new InvalidOperationException("This request is no longer awaiting approval.");
        }

        var step = await unitOfWork.Repository<ApprovalStep>().Query()
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.LevelId == request.CurrentLevelId &&
                                       x.ApproverUserId == actorUserId && x.IsActive && x.Action == null, ct)
            ?? throw new UnauthorizedAccessException("You do not have a pending approval step for this request.");

        var level = await unitOfWork.Repository<ApprovalLevel>().GetByIdAsync(step.LevelId, ct)
            ?? throw new InvalidOperationException("Approval level not found.");

        var now = DateTimeOffset.UtcNow;

        if (actionRequest.DelegateUserId.HasValue && actionRequest.DelegateUserId.Value != actorUserId)
        {
            step.Action = ApprovalStepAction.Delegated;
            step.ActionAt = now;
            step.Comment = actionRequest.Comment;
            step.IsActive = false;
            unitOfWork.Repository<ApprovalStep>().Update(step);

            var delegateStep = new ApprovalStep
            {
                RequestId = requestId,
                LevelId = step.LevelId,
                LevelOrder = step.LevelOrder,
                ApproverUserId = actionRequest.DelegateUserId.Value,
                IsDelegated = true,
                DelegatedFromUserId = actorUserId,
                DueAt = step.DueAt,
                IsActive = true,
                CreatedBy = "system"
            };
            await unitOfWork.Repository<ApprovalStep>().AddAsync(delegateStep, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await WriteAuditAsync(requestId, step.Id, actorUserId, "DELEGATED", request.Module, request.Status, request.Status, actionRequest.Comment, ct);
            await notificationService.NotifyAsync(requestId, delegateStep.Id, actionRequest.DelegateUserId.Value, ApprovalNotificationType.Delegated,
                $"Approval delegated to you: {request.Subject}",
                $"Request {request.RequestNo} ({request.Subject}) has been delegated to you.", ct);

            return await LoadRequestDtoAsync(requestId, ct);
        }

        if (action == ApprovalStepAction.Rejected && request.Template!.RequireCommentOnReject && string.IsNullOrWhiteSpace(actionRequest.Comment))
        {
            throw new InvalidOperationException("A comment is required when rejecting.");
        }

        step.Action = action;
        step.ActionAt = now;
        step.Comment = actionRequest.Comment;
        unitOfWork.Repository<ApprovalStep>().Update(step);

        if (action == ApprovalStepAction.Rejected)
        {
            var otherActiveSteps = await unitOfWork.Repository<ApprovalStep>().Query()
                .Where(x => x.RequestId == requestId && x.IsActive && x.Action == null && x.Id != step.Id)
                .ToListAsync(ct);
            foreach (var other in otherActiveSteps)
            {
                other.IsActive = false;
                unitOfWork.Repository<ApprovalStep>().Update(other);
            }

            var oldStatus = request.Status;
            request.Status = ApprovalRequestStatus.Rejected;
            request.FinalActionAt = now;
            request.FinalActionBy = actorUserId;
            unitOfWork.Repository<ApprovalRequest>().Update(request);
            await unitOfWork.SaveChangesAsync(ct);

            await WriteAuditAsync(requestId, step.Id, actorUserId, "REJECTED", request.Module, oldStatus, ApprovalRequestStatus.Rejected, actionRequest.Comment, ct);
            await notificationService.NotifyAsync(requestId, step.Id, request.RequestedBy, ApprovalNotificationType.Rejected,
                $"Request rejected: {request.Subject}",
                $"Your request {request.RequestNo} ({request.Subject}) was rejected. Reason: {actionRequest.Comment}", ct);
            await DispatchApprovedOrRejectedCallbackAsync(request, approved: false, actionRequest.Comment, actorUserId, ct);

            return await LoadRequestDtoAsync(requestId, ct);
        }

        if (request.Template!.ApprovalType == ApprovalType.AnyOne)
        {
            var siblingSteps = await unitOfWork.Repository<ApprovalStep>().Query()
                .Where(x => x.RequestId == requestId && x.LevelId == step.LevelId && x.IsActive && x.Action == null && x.Id != step.Id)
                .ToListAsync(ct);
            foreach (var sibling in siblingSteps)
            {
                sibling.IsActive = false;
                unitOfWork.Repository<ApprovalStep>().Update(sibling);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        var approvedCount = await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .CountAsync(x => x.RequestId == requestId && x.LevelId == step.LevelId && x.Action == ApprovalStepAction.Approved, ct);

        await WriteAuditAsync(requestId, step.Id, actorUserId, "STEP_APPROVED", request.Module, request.Status, request.Status, actionRequest.Comment, ct);

        if (approvedCount < level.MinApproversRequired)
        {
            return await LoadRequestDtoAsync(requestId, ct);
        }

        var nextLevel = await unitOfWork.Repository<ApprovalLevel>().Query().AsNoTracking()
            .Where(x => x.TemplateId == request.TemplateId && x.IsActive && x.LevelOrder > level.LevelOrder)
            .OrderBy(x => x.LevelOrder)
            .FirstOrDefaultAsync(ct);

        if (nextLevel is not null)
        {
            await ActivateLevelAsync(request, request.Template!, nextLevel, ct);
            return await LoadRequestDtoAsync(requestId, ct);
        }

        var finalOldStatus = request.Status;
        request.Status = ApprovalRequestStatus.Approved;
        request.FinalActionAt = now;
        request.FinalActionBy = actorUserId;
        unitOfWork.Repository<ApprovalRequest>().Update(request);
        await unitOfWork.SaveChangesAsync(ct);

        await WriteAuditAsync(requestId, step.Id, actorUserId, "APPROVED", request.Module, finalOldStatus, ApprovalRequestStatus.Approved, actionRequest.Comment, ct);
        await notificationService.NotifyAsync(requestId, step.Id, request.RequestedBy, ApprovalNotificationType.Approved,
            $"Request approved: {request.Subject}",
            $"Your request {request.RequestNo} ({request.Subject}) has been fully approved.", ct);
        await DispatchApprovedOrRejectedCallbackAsync(request, approved: true, actionRequest.Comment, actorUserId, ct);

        return await LoadRequestDtoAsync(requestId, ct);
    }

    private async Task ActivateLevelAsync(ApprovalRequest request, ApprovalTemplate template, ApprovalLevel level, CancellationToken ct)
    {
        var approverUserIds = await ResolveApproverUserIdsAsync(level, request.RequestedBy, ct);
        if (approverUserIds.Count == 0)
        {
            throw new InvalidOperationException($"No approvers could be resolved for level '{level.LevelName}'.");
        }

        var now = DateTimeOffset.UtcNow;
        var dueAt = now.AddHours(template.SlaHours);
        var activeSteps = new List<ApprovalStep>();

        foreach (var approverUserId in approverUserIds)
        {
            var originalStep = new ApprovalStep
            {
                RequestId = request.Id,
                LevelId = level.Id,
                LevelOrder = level.LevelOrder,
                ApproverUserId = approverUserId,
                DueAt = dueAt,
                IsActive = true,
                CreatedBy = "system"
            };

            var delegation = await FindActiveDelegationAsync(approverUserId, template.Id, ct);
            if (delegation is not null)
            {
                originalStep.IsActive = false;
                originalStep.Action = ApprovalStepAction.Delegated;
                originalStep.ActionAt = now;
                originalStep.Comment = $"Auto-delegated to user #{delegation.DelegateUserId}.";
                await unitOfWork.Repository<ApprovalStep>().AddAsync(originalStep, ct);

                var delegateStep = new ApprovalStep
                {
                    RequestId = request.Id,
                    LevelId = level.Id,
                    LevelOrder = level.LevelOrder,
                    ApproverUserId = delegation.DelegateUserId,
                    IsDelegated = true,
                    DelegatedFromUserId = approverUserId,
                    DueAt = dueAt,
                    IsActive = true,
                    CreatedBy = "system"
                };
                await unitOfWork.Repository<ApprovalStep>().AddAsync(delegateStep, ct);
                activeSteps.Add(delegateStep);
            }
            else
            {
                await unitOfWork.Repository<ApprovalStep>().AddAsync(originalStep, ct);
                activeSteps.Add(originalStep);
            }
        }

        request.CurrentLevelId = level.Id;
        request.DueAt = dueAt;
        request.Status = ApprovalRequestStatus.InProgress;
        unitOfWork.Repository<ApprovalRequest>().Update(request);

        await unitOfWork.SaveChangesAsync(ct);

        await WriteAuditAsync(request.Id, null, request.RequestedBy, "LEVEL_ACTIVATED", request.Module, null, ApprovalRequestStatus.InProgress, $"Level '{level.LevelName}' activated.", ct);

        foreach (var step in activeSteps)
        {
            await notificationService.NotifyAsync(request.Id, step.Id, step.ApproverUserId, ApprovalNotificationType.NewRequest,
                $"Approval needed: {request.Subject}",
                $"Request {request.RequestNo} ({request.Subject}) is waiting for your approval at level '{level.LevelName}'. Due by {dueAt:yyyy-MM-dd HH:mm} UTC.",
                ct);
        }
    }

    private async Task<List<int>> ResolveApproverUserIdsAsync(ApprovalLevel level, int requestedByUserId, CancellationToken ct)
    {
        switch (level.ApproverType)
        {
            case ApprovalApproverType.Role:
            {
                if (level.ApproverRoleId is null)
                {
                    throw new InvalidOperationException($"Level '{level.LevelName}' has no role configured.");
                }

                return await unitOfWork.Repository<SysUserRole>().Query().AsNoTracking()
                    .Where(x => x.RoleId == level.ApproverRoleId.Value && x.User.IsActive)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync(ct);
            }
            case ApprovalApproverType.Position:
            {
                if (level.ApproverPositionId is null)
                {
                    throw new InvalidOperationException($"Level '{level.LevelName}' has no position configured.");
                }

                return await unitOfWork.Repository<HrEmployee>().Query().AsNoTracking()
                    .Where(x => x.PositionId == level.ApproverPositionId.Value &&
                                x.EmploymentStatus == EmploymentStatus.Active &&
                                x.UserId != null)
                    .Select(x => x.UserId!.Value)
                    .Distinct()
                    .ToListAsync(ct);
            }
            case ApprovalApproverType.SpecificUser:
            {
                if (level.ApproverUserId is null)
                {
                    throw new InvalidOperationException($"Level '{level.LevelName}' has no specific user configured.");
                }

                return [level.ApproverUserId.Value];
            }
            case ApprovalApproverType.DirectSuperior:
            {
                var requesterEmployee = await unitOfWork.Repository<HrEmployee>().Query().AsNoTracking()
                    .Include(x => x.Department)
                    .FirstOrDefaultAsync(x => x.UserId == requestedByUserId, ct)
                    ?? throw new InvalidOperationException("Cannot resolve direct superior: the requester has no linked employee record.");

                var managerId = requesterEmployee.Department?.ManagerId
                    ?? throw new InvalidOperationException("Cannot resolve direct superior: the requester's department has no manager assigned.");

                var managerEmployee = await unitOfWork.Repository<HrEmployee>().Query().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == managerId, ct)
                    ?? throw new InvalidOperationException("Cannot resolve direct superior: manager employee record not found.");

                if (managerEmployee.UserId is null)
                {
                    throw new InvalidOperationException("Cannot resolve direct superior: the manager has no linked user account.");
                }

                return [managerEmployee.UserId.Value];
            }
            default:
                throw new InvalidOperationException($"Unsupported approver type '{level.ApproverType}'.");
        }
    }

    private async Task<ApprovalDelegation?> FindActiveDelegationAsync(int delegatorUserId, int templateId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await unitOfWork.Repository<ApprovalDelegation>().Query().AsNoTracking()
            .Where(x => x.DelegatorUserId == delegatorUserId && x.IsActive &&
                        x.StartDate <= today && x.EndDate >= today &&
                        (x.TemplateId == null || x.TemplateId == templateId))
            .OrderBy(x => x.TemplateId == null ? 1 : 0)
            .FirstOrDefaultAsync(ct);
    }

    private async Task DispatchApprovedOrRejectedCallbackAsync(ApprovalRequest request, bool approved, string? comment, int actorUserId, CancellationToken ct)
    {
        var callback = callbackServices.FirstOrDefault(x => string.Equals(x.ReferenceType, request.ReferenceType, StringComparison.OrdinalIgnoreCase));
        if (callback is null)
        {
            return;
        }

        if (approved)
        {
            await callback.OnApprovedAsync(request.ReferenceId, actorUserId, ct);
        }
        else
        {
            await callback.OnRejectedAsync(request.ReferenceId, actorUserId, comment, ct);
        }
    }

    private async Task WriteAuditAsync(int requestId, int? stepId, int actorUserId, string action, string module, ApprovalRequestStatus? oldStatus, ApprovalRequestStatus? newStatus, string? comment, CancellationToken ct)
    {
        var log = new ApprovalAuditLog
        {
            RequestId = requestId,
            StepId = stepId,
            ActorUserId = actorUserId,
            Action = action,
            Module = module,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = comment,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await unitOfWork.Repository<ApprovalAuditLog>().AddAsync(log, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<string> GenerateRequestNoAsync(DateTimeOffset date, CancellationToken ct)
    {
        var prefix = $"APV-{date.Year}-";

        var existingNos = await unitOfWork.Repository<ApprovalRequest>().Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.RequestNo.StartsWith(prefix))
            .Select(x => x.RequestNo)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var requestNo in existingNos)
        {
            if (requestNo.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = requestNo[prefix.Length..];
            if (int.TryParse(suffix, out var parsed) && parsed > maxSequence)
            {
                maxSequence = parsed;
            }
        }

        return $"{prefix}{maxSequence + 1:D5}";
    }

    private async Task<ApprovalRequestDto> LoadRequestDtoAsync(int requestId, CancellationToken ct)
    {
        var request = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.CurrentLevel)
            .Include(x => x.RequestedByUser)
            .Include(x => x.FinalActionByUser)
            .FirstOrDefaultAsync(x => x.Id == requestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        return MapRequest(request);
    }

    private static ApprovalRequestDto MapRequest(ApprovalRequest entity) => new()
    {
        Id = entity.Id,
        RequestNo = entity.RequestNo,
        TemplateId = entity.TemplateId,
        TemplateName = entity.Template?.Name ?? string.Empty,
        Module = entity.Module,
        ReferenceType = entity.ReferenceType,
        ReferenceId = entity.ReferenceId,
        Subject = entity.Subject,
        Amount = entity.Amount,
        RequestedBy = entity.RequestedBy,
        RequestedByName = entity.RequestedByUser?.FullName ?? string.Empty,
        RequestedAt = entity.RequestedAt,
        CurrentLevelId = entity.CurrentLevelId,
        CurrentLevelName = entity.CurrentLevel?.LevelName,
        DueAt = entity.DueAt,
        Status = entity.Status,
        FinalActionAt = entity.FinalActionAt,
        FinalActionBy = entity.FinalActionBy,
        FinalActionByName = entity.FinalActionByUser?.FullName,
        Notes = entity.Notes
    };
}

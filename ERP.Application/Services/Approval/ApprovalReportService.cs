using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Entities.Approval;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Approval;

public sealed class ApprovalReportService(IUnitOfWork unitOfWork) : IApprovalReportService
{
    public async Task<ApprovalDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStartUtc = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var waitingMyAction = await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .CountAsync(x => x.ApproverUserId == userId && x.IsActive && x.Action == null, ct);

        var myOpenRequests = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .CountAsync(x => x.RequestedBy == userId &&
                (x.Status == ApprovalRequestStatus.Pending || x.Status == ApprovalRequestStatus.InProgress), ct);

        var overdueSteps = await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .CountAsync(x => x.IsActive && x.Action == null && x.DueAt < now, ct);

        var approvedToday = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .CountAsync(x => x.Status == ApprovalRequestStatus.Approved && x.FinalActionAt != null && x.FinalActionAt >= todayStartUtc, ct);

        var rejectedToday = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .CountAsync(x => x.Status == ApprovalRequestStatus.Rejected && x.FinalActionAt != null && x.FinalActionAt >= todayStartUtc, ct);

        var activeTemplates = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .CountAsync(x => x.IsActive, ct);

        var activeDelegations = await unitOfWork.Repository<ApprovalDelegation>().Query().AsNoTracking()
            .CountAsync(x => x.IsActive && x.StartDate <= today && x.EndDate >= today, ct);

        return new ApprovalDashboardDto
        {
            WaitingMyActionCount = waitingMyAction,
            MyOpenRequestCount = myOpenRequests,
            OverdueStepCount = overdueSteps,
            ApprovedTodayCount = approvedToday,
            RejectedTodayCount = rejectedToday,
            ActiveTemplateCount = activeTemplates,
            ActiveDelegationCount = activeDelegations
        };
    }

    public async Task<PagedResult<ApprovalSlaReportDto>> GetSlaReportAsync(ApprovalSlaReportPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .Include(x => x.Request!).ThenInclude(r => r.Template)
            .Where(x => x.Action == ApprovalStepAction.Approved || x.Action == ApprovalStepAction.Rejected);

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            var module = request.Module.Trim().ToLower();
            query = query.Where(x => x.Request!.Module.ToLower() == module);
        }

        if (request.TemplateId.HasValue)
        {
            query = query.Where(x => x.Request!.TemplateId == request.TemplateId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            var from = new DateTimeOffset(request.DateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.ActionAt >= from);
        }

        if (request.DateTo.HasValue)
        {
            var to = new DateTimeOffset(request.DateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.ActionAt <= to);
        }

        var steps = await query.ToListAsync(ct);

        var grouped = steps
            .Where(x => x.Request?.Template is not null)
            .GroupBy(x => x.Request!.Template!)
            .Select(g =>
            {
                var count = g.Count();
                var withinSla = g.Count(x => x.ActionAt.HasValue && x.ActionAt.Value <= x.DueAt);
                var overdue = count - withinSla;
                var avgResponseHours = g.Average(x => x.ActionAt.HasValue ? (x.ActionAt.Value - x.CreatedAt).TotalHours : 0);

                return new ApprovalSlaReportDto
                {
                    Module = g.Key.Module,
                    TemplateCode = g.Key.Code,
                    TemplateName = g.Key.Name,
                    SlaHours = g.Key.SlaHours,
                    AverageResponseHours = Math.Round((decimal)avgResponseHours, 2),
                    TotalSteps = count,
                    WithinSlaCount = withinSla,
                    OverdueCount = overdue,
                    ComplianceRate = count == 0 ? 0 : Math.Round((decimal)withinSla / count * 100, 2)
                };
            })
            .OrderBy(x => x.TemplateCode)
            .ToList();

        var total = grouped.Count;
        var items = grouped.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return PagedResult<ApprovalSlaReportDto>.Create(items, total, page, pageSize);
    }

    public async Task<PagedResult<ApprovalTemplateReportDto>> GetTemplateReportAsync(ApprovalTemplateReportPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .Include(x => x.Template)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            var module = request.Module.Trim().ToLower();
            query = query.Where(x => x.Module.ToLower() == module);
        }

        if (request.TemplateId.HasValue)
        {
            query = query.Where(x => x.TemplateId == request.TemplateId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            var from = new DateTimeOffset(request.DateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.RequestedAt >= from);
        }

        if (request.DateTo.HasValue)
        {
            var to = new DateTimeOffset(request.DateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.RequestedAt <= to);
        }

        var requests = await query.ToListAsync(ct);

        var grouped = requests
            .Where(x => x.Template is not null)
            .GroupBy(x => x.Template!)
            .Select(g => new ApprovalTemplateReportDto
            {
                TemplateId = g.Key.Id,
                TemplateCode = g.Key.Code,
                TemplateName = g.Key.Name,
                Module = g.Key.Module,
                TotalRequests = g.Count(),
                ApprovedCount = g.Count(x => x.Status == ApprovalRequestStatus.Approved),
                RejectedCount = g.Count(x => x.Status == ApprovalRequestStatus.Rejected),
                CancelledCount = g.Count(x => x.Status == ApprovalRequestStatus.Cancelled),
                PendingCount = g.Count(x => x.Status == ApprovalRequestStatus.Pending || x.Status == ApprovalRequestStatus.InProgress),
                AverageDurationHours = Math.Round((decimal)g.Where(x => x.FinalActionAt.HasValue)
                    .Select(x => (x.FinalActionAt!.Value - x.RequestedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average(), 2)
            })
            .OrderBy(x => x.TemplateCode)
            .ToList();

        var total = grouped.Count;
        var items = grouped.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return PagedResult<ApprovalTemplateReportDto>.Create(items, total, page, pageSize);
    }

    public async Task<PagedResult<ApprovalAuditLogDto>> GetAuditLogsAsync(ApprovalAuditPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalAuditLog>().Query().AsNoTracking().AsQueryable();

        if (request.RequestId.HasValue)
        {
            query = query.Where(x => x.RequestId == request.RequestId.Value);
        }

        if (request.ActorUserId.HasValue)
        {
            query = query.Where(x => x.ActorUserId == request.ActorUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim().ToUpper();
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            var module = request.Module.Trim().ToLower();
            query = query.Where(x => x.Module.ToLower() == module);
        }

        if (request.DateFrom.HasValue)
        {
            var from = new DateTimeOffset(request.DateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.CreatedAt >= from);
        }

        if (request.DateTo.HasValue)
        {
            var to = new DateTimeOffset(request.DateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.CreatedAt <= to);
        }

        var total = await query.CountAsync(ct);

        var entities = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var actorIds = entities.Select(x => x.ActorUserId).Distinct().ToList();
        var actorNames = await unitOfWork.Repository<SysUser>().Query().AsNoTracking()
            .Where(x => actorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

        var items = entities.Select(x => new ApprovalAuditLogDto
        {
            Id = x.Id,
            RequestId = x.RequestId,
            StepId = x.StepId,
            ActorUserId = x.ActorUserId,
            ActorUserName = actorNames.TryGetValue(x.ActorUserId, out var name) ? name : string.Empty,
            Action = x.Action,
            Module = x.Module,
            OldStatus = x.OldStatus,
            NewStatus = x.NewStatus,
            IpAddress = x.IpAddress,
            UserAgent = x.UserAgent,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt
        }).ToList();

        return PagedResult<ApprovalAuditLogDto>.Create(items, total, page, pageSize);
    }
}

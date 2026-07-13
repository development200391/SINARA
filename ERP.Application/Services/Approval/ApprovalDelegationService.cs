using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Entities.Approval;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Approval;

public sealed class ApprovalDelegationService(IUnitOfWork unitOfWork) : IApprovalDelegationService
{
    public async Task<PagedResult<ApprovalDelegationDto>> GetDelegationsPagedAsync(ApprovalDelegationPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalDelegation>().Query().AsNoTracking()
            .Include(x => x.DelegatorUser)
            .Include(x => x.DelegateUser)
            .Include(x => x.Template)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.DelegatorUser!.FullName.ToLower().Contains(search) ||
                x.DelegateUser!.FullName.ToLower().Contains(search));
        }

        if (request.DelegatorUserId.HasValue)
        {
            query = query.Where(x => x.DelegatorUserId == request.DelegatorUserId.Value);
        }

        if (request.DelegateUserId.HasValue)
        {
            query = query.Where(x => x.DelegateUserId == request.DelegateUserId.Value);
        }

        if (request.TemplateId.HasValue)
        {
            query = query.Where(x => x.TemplateId == request.TemplateId.Value);
        }

        if (request.EffectiveDateFrom.HasValue)
        {
            query = query.Where(x => x.EndDate >= request.EffectiveDateFrom.Value);
        }

        if (request.EffectiveDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= request.EffectiveDateTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var total = await query.CountAsync(ct);

        var entities = await query.OrderByDescending(x => x.StartDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = entities.Select(MapDelegation).ToList();

        return PagedResult<ApprovalDelegationDto>.Create(items, total, page, pageSize);
    }

    public async Task<ApprovalDelegationDto?> GetDelegationByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<ApprovalDelegation>().Query().AsNoTracking()
            .Include(x => x.DelegatorUser)
            .Include(x => x.DelegateUser)
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapDelegation(entity);
    }

    public async Task<ApprovalDelegationDto> CreateDelegationAsync(ApprovalDelegationDto request, CancellationToken ct = default)
    {
        await ValidateAsync(request, ct);

        var entity = new ApprovalDelegation
        {
            DelegatorUserId = request.DelegatorUserId,
            DelegateUserId = request.DelegateUserId,
            TemplateId = request.TemplateId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason?.Trim(),
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<ApprovalDelegation>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetDelegationByIdAsync(entity.Id, ct) ?? MapDelegation(entity);
    }

    public async Task<ApprovalDelegationDto> UpdateDelegationAsync(int id, ApprovalDelegationDto request, CancellationToken ct = default)
    {
        await ValidateAsync(request, ct);

        var entity = await unitOfWork.Repository<ApprovalDelegation>().GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Delegation not found.");

        entity.DelegatorUserId = request.DelegatorUserId;
        entity.DelegateUserId = request.DelegateUserId;
        entity.TemplateId = request.TemplateId;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Reason = request.Reason?.Trim();
        entity.IsActive = request.IsActive;

        unitOfWork.Repository<ApprovalDelegation>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetDelegationByIdAsync(entity.Id, ct) ?? MapDelegation(entity);
    }

    public async Task RevokeDelegationAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<ApprovalDelegation>().GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Delegation not found.");

        entity.IsActive = false;
        unitOfWork.Repository<ApprovalDelegation>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ApprovalOptionDto>> GetApproverOptionsAsync(CancellationToken ct = default)
    {
        var users = await unitOfWork.Repository<SysUser>().Query().AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .ToListAsync(ct);

        return users.Select(x => new ApprovalOptionDto { Id = x.Id, Label = x.FullName }).ToList();
    }

    private async Task ValidateAsync(ApprovalDelegationDto request, CancellationToken ct)
    {
        if (request.DelegatorUserId <= 0 || request.DelegateUserId <= 0)
        {
            throw new InvalidOperationException("Delegator and delegate are required.");
        }

        if (request.DelegatorUserId == request.DelegateUserId)
        {
            throw new InvalidOperationException("A user cannot delegate to themselves.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date cannot be earlier than start date.");
        }

        var delegatorExists = await unitOfWork.Repository<SysUser>().Query().AsNoTracking().AnyAsync(x => x.Id == request.DelegatorUserId, ct);
        if (!delegatorExists)
        {
            throw new InvalidOperationException("Delegator user not found.");
        }

        var delegateExists = await unitOfWork.Repository<SysUser>().Query().AsNoTracking().AnyAsync(x => x.Id == request.DelegateUserId, ct);
        if (!delegateExists)
        {
            throw new InvalidOperationException("Delegate user not found.");
        }
    }

    private static ApprovalDelegationDto MapDelegation(ApprovalDelegation entity) => new()
    {
        Id = entity.Id,
        DelegatorUserId = entity.DelegatorUserId,
        DelegatorName = entity.DelegatorUser?.FullName ?? string.Empty,
        DelegateUserId = entity.DelegateUserId,
        DelegateName = entity.DelegateUser?.FullName ?? string.Empty,
        TemplateId = entity.TemplateId,
        TemplateCode = entity.Template?.Code,
        TemplateName = entity.Template?.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Reason = entity.Reason,
        IsActive = entity.IsActive
    };
}

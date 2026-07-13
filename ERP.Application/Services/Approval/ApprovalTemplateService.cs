using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Entities.Approval;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Approval;

public sealed class ApprovalTemplateService(IUnitOfWork unitOfWork) : IApprovalTemplateService
{
    public async Task<PagedResult<ApprovalTemplateDto>> GetTemplatesPagedAsync(ApprovalTemplatePagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking().Include(x => x.Levels).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(search) || x.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(name));
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

        if (request.ApprovalType.HasValue)
        {
            query = query.Where(x => x.ApprovalType == request.ApprovalType.Value);
        }

        if (request.MinAmountFrom.HasValue)
        {
            query = query.Where(x => x.MinAmount >= request.MinAmountFrom.Value);
        }

        if (request.MinAmountTo.HasValue)
        {
            query = query.Where(x => x.MinAmount <= request.MinAmountTo.Value);
        }

        if (request.MaxAmountFrom.HasValue)
        {
            query = query.Where(x => x.MaxAmount >= request.MaxAmountFrom.Value);
        }

        if (request.MaxAmountTo.HasValue)
        {
            query = query.Where(x => x.MaxAmount <= request.MaxAmountTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var total = await query.CountAsync(ct);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.SortDirection == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "module" => request.SortDirection == "asc" ? query.OrderBy(x => x.Module) : query.OrderByDescending(x => x.Module),
            _ => request.SortDirection == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code)
        };

        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = entities.Select(MapTemplate).ToList();

        return PagedResult<ApprovalTemplateDto>.Create(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<ApprovalOptionDto>> GetTemplateOptionsAsync(CancellationToken ct = default)
    {
        var templates = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return templates.Select(x => new ApprovalOptionDto { Id = x.Id, Label = $"{x.Code} - {x.Name}" }).ToList();
    }

    public async Task<ApprovalTemplateDto?> GetTemplateByIdAsync(int id, CancellationToken ct = default)
    {
        var template = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .Include(x => x.Levels)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return template is null ? null : MapTemplate(template);
    }

    public async Task<ApprovalTemplateDto> CreateTemplateAsync(ApprovalTemplateDto request, CancellationToken ct = default)
    {
        ValidateTemplate(request);

        var codeExists = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower(), ct);
        if (codeExists)
        {
            throw new InvalidOperationException($"Template code '{request.Code}' already exists.");
        }

        var entity = new ApprovalTemplate
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Module = request.Module.Trim(),
            ReferenceType = request.ReferenceType.Trim(),
            ApprovalType = request.ApprovalType,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            AutoApproveBelow = request.AutoApproveBelow,
            SlaHours = request.SlaHours,
            AllowDelegation = request.AllowDelegation,
            RequireCommentOnReject = request.RequireCommentOnReject,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<ApprovalTemplate>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapTemplate(entity);
    }

    public async Task<ApprovalTemplateDto> UpdateTemplateAsync(int id, ApprovalTemplateDto request, CancellationToken ct = default)
    {
        ValidateTemplate(request);

        var entity = await unitOfWork.Repository<ApprovalTemplate>().GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Approval template not found.");

        var codeExists = await unitOfWork.Repository<ApprovalTemplate>().Query().AsNoTracking()
            .AnyAsync(x => x.Id != id && x.Code.ToLower() == request.Code.Trim().ToLower(), ct);
        if (codeExists)
        {
            throw new InvalidOperationException($"Template code '{request.Code}' already exists.");
        }

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Module = request.Module.Trim();
        entity.ReferenceType = request.ReferenceType.Trim();
        entity.ApprovalType = request.ApprovalType;
        entity.MinAmount = request.MinAmount;
        entity.MaxAmount = request.MaxAmount;
        entity.AutoApproveBelow = request.AutoApproveBelow;
        entity.SlaHours = request.SlaHours;
        entity.AllowDelegation = request.AllowDelegation;
        entity.RequireCommentOnReject = request.RequireCommentOnReject;
        entity.IsActive = request.IsActive;

        unitOfWork.Repository<ApprovalTemplate>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return MapTemplate(entity);
    }

    public async Task SetTemplateActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<ApprovalTemplate>().GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Approval template not found.");

        entity.IsActive = isActive;
        unitOfWork.Repository<ApprovalTemplate>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteTemplateAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<ApprovalTemplate>().GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Approval template not found.");

        var hasRequests = await unitOfWork.Repository<ApprovalRequest>().Query().AsNoTracking()
            .AnyAsync(x => x.TemplateId == id, ct);
        if (hasRequests)
        {
            throw new InvalidOperationException("This template cannot be deleted because approval requests already reference it.");
        }

        unitOfWork.Repository<ApprovalTemplate>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ApprovalLevelDto>> GetLevelsAsync(int templateId, CancellationToken ct = default)
    {
        var levels = await unitOfWork.Repository<ApprovalLevel>().Query().AsNoTracking()
            .Where(x => x.TemplateId == templateId)
            .OrderBy(x => x.LevelOrder)
            .ToListAsync(ct);

        return levels.Select(MapLevel).ToList();
    }

    public async Task<ApprovalLevelDto?> GetLevelByIdAsync(int templateId, int levelId, CancellationToken ct = default)
    {
        var level = await unitOfWork.Repository<ApprovalLevel>().Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == levelId && x.TemplateId == templateId, ct);

        return level is null ? null : MapLevel(level);
    }

    public async Task<ApprovalLevelDto> CreateLevelAsync(int templateId, ApprovalLevelDto request, CancellationToken ct = default)
    {
        var template = await unitOfWork.Repository<ApprovalTemplate>().GetByIdAsync(templateId, ct)
            ?? throw new InvalidOperationException("Approval template not found.");

        ValidateLevel(request);

        var orderExists = await unitOfWork.Repository<ApprovalLevel>().Query().AsNoTracking()
            .AnyAsync(x => x.TemplateId == templateId && x.LevelOrder == request.LevelOrder, ct);
        if (orderExists)
        {
            throw new InvalidOperationException($"Level order {request.LevelOrder} already exists for this template.");
        }

        var entity = new ApprovalLevel
        {
            TemplateId = template.Id,
            LevelOrder = request.LevelOrder,
            LevelName = request.LevelName.Trim(),
            ApproverType = request.ApproverType,
            ApproverRoleId = request.ApproverRoleId,
            ApproverPositionId = request.ApproverPositionId,
            ApproverUserId = request.ApproverUserId,
            MinApproversRequired = request.MinApproversRequired,
            EscalationHours = request.EscalationHours,
            EscalateToLevelId = request.EscalateToLevelId,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<ApprovalLevel>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapLevel(entity);
    }

    public async Task<ApprovalLevelDto> UpdateLevelAsync(int templateId, int levelId, ApprovalLevelDto request, CancellationToken ct = default)
    {
        ValidateLevel(request);

        var entity = await unitOfWork.Repository<ApprovalLevel>().Query()
            .FirstOrDefaultAsync(x => x.Id == levelId && x.TemplateId == templateId, ct)
            ?? throw new InvalidOperationException("Approval level not found.");

        var orderExists = await unitOfWork.Repository<ApprovalLevel>().Query().AsNoTracking()
            .AnyAsync(x => x.Id != levelId && x.TemplateId == templateId && x.LevelOrder == request.LevelOrder, ct);
        if (orderExists)
        {
            throw new InvalidOperationException($"Level order {request.LevelOrder} already exists for this template.");
        }

        entity.LevelOrder = request.LevelOrder;
        entity.LevelName = request.LevelName.Trim();
        entity.ApproverType = request.ApproverType;
        entity.ApproverRoleId = request.ApproverRoleId;
        entity.ApproverPositionId = request.ApproverPositionId;
        entity.ApproverUserId = request.ApproverUserId;
        entity.MinApproversRequired = request.MinApproversRequired;
        entity.EscalationHours = request.EscalationHours;
        entity.EscalateToLevelId = request.EscalateToLevelId;
        entity.IsActive = request.IsActive;

        unitOfWork.Repository<ApprovalLevel>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return MapLevel(entity);
    }

    public async Task DeleteLevelAsync(int templateId, int levelId, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<ApprovalLevel>().Query()
            .FirstOrDefaultAsync(x => x.Id == levelId && x.TemplateId == templateId, ct)
            ?? throw new InvalidOperationException("Approval level not found.");

        var hasSteps = await unitOfWork.Repository<ApprovalStep>().Query().AsNoTracking()
            .AnyAsync(x => x.LevelId == levelId, ct);
        if (hasSteps)
        {
            throw new InvalidOperationException("This level cannot be deleted because approval steps already reference it.");
        }

        unitOfWork.Repository<ApprovalLevel>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static void ValidateTemplate(ApprovalTemplateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new InvalidOperationException("Template code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Template name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Module))
        {
            throw new InvalidOperationException("Module is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            throw new InvalidOperationException("Reference type is required.");
        }

        if (request.MinAmount.HasValue && request.MaxAmount.HasValue && request.MinAmount > request.MaxAmount)
        {
            throw new InvalidOperationException("Minimum amount cannot be greater than maximum amount.");
        }

        if (request.SlaHours <= 0)
        {
            throw new InvalidOperationException("SLA hours must be greater than 0.");
        }
    }

    private static void ValidateLevel(ApprovalLevelDto request)
    {
        if (string.IsNullOrWhiteSpace(request.LevelName))
        {
            throw new InvalidOperationException("Level name is required.");
        }

        if (request.LevelOrder <= 0)
        {
            throw new InvalidOperationException("Level order must be greater than 0.");
        }

        if (request.MinApproversRequired <= 0)
        {
            throw new InvalidOperationException("Minimum approvers required must be greater than 0.");
        }

        switch (request.ApproverType)
        {
            case ERP.Domain.Enums.Approval.ApprovalApproverType.Role when request.ApproverRoleId is null:
                throw new InvalidOperationException("Approver role is required for approver type 'Role'.");
            case ERP.Domain.Enums.Approval.ApprovalApproverType.Position when request.ApproverPositionId is null:
                throw new InvalidOperationException("Approver position is required for approver type 'Position'.");
            case ERP.Domain.Enums.Approval.ApprovalApproverType.SpecificUser when request.ApproverUserId is null:
                throw new InvalidOperationException("Approver user is required for approver type 'SpecificUser'.");
        }
    }

    private static ApprovalTemplateDto MapTemplate(ApprovalTemplate entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Module = entity.Module,
        ReferenceType = entity.ReferenceType,
        ApprovalType = entity.ApprovalType,
        MinAmount = entity.MinAmount,
        MaxAmount = entity.MaxAmount,
        AutoApproveBelow = entity.AutoApproveBelow,
        SlaHours = entity.SlaHours,
        AllowDelegation = entity.AllowDelegation,
        RequireCommentOnReject = entity.RequireCommentOnReject,
        IsActive = entity.IsActive,
        LevelCount = entity.Levels?.Count ?? 0
    };

    private static ApprovalLevelDto MapLevel(ApprovalLevel entity) => new()
    {
        Id = entity.Id,
        TemplateId = entity.TemplateId,
        LevelOrder = entity.LevelOrder,
        LevelName = entity.LevelName,
        ApproverType = entity.ApproverType,
        ApproverRoleId = entity.ApproverRoleId,
        ApproverPositionId = entity.ApproverPositionId,
        ApproverUserId = entity.ApproverUserId,
        MinApproversRequired = entity.MinApproversRequired,
        EscalationHours = entity.EscalationHours,
        EscalateToLevelId = entity.EscalateToLevelId,
        IsActive = entity.IsActive
    };
}

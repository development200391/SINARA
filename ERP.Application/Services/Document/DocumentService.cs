using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Application.Options;
using ERP.Domain.Entities.Document;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERP.Application.Services.Document;

public sealed class DocumentService(IUnitOfWork unitOfWork, IDocumentStorageService storageService, IOptions<DocumentSettings> options) : IDocumentService
{
    private readonly DocumentSettings _settings = options.Value;

    private static readonly HashSet<string> AllowedReferenceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "hr_leave_requests"
    };

    public async Task<IReadOnlyList<DocumentDto>> GetByReferenceAsync(string referenceType, int referenceId, int currentUserId, CancellationToken ct = default)
    {
        await EnsureReferenceAccessAsync(referenceType, referenceId, currentUserId, requireMutable: false, ct);

        return await unitOfWork.Repository<DocDocument>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.UploadedByUser)
            .Where(x => x.ReferenceType == referenceType && x.ReferenceId == referenceId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => MapDocument(x))
            .ToListAsync(ct);
    }

    public async Task<DocumentDto> UploadAsync(UploadDocumentRequest request, int currentUserId, CancellationToken ct = default)
    {
        if (request.FileSizeBytes <= 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        if (request.FileSizeBytes > _settings.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File must be {_settings.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
        }

        var extension = Path.GetExtension(request.OriginalFileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"File type not allowed. Allowed: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        await EnsureReferenceAccessAsync(request.ReferenceType, request.ReferenceId, currentUserId, requireMutable: true, ct);

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await unitOfWork.Repository<DocDocumentCategory>()
                .Query()
                .AnyAsync(x => x.Id == request.CategoryId.Value && x.IsActive, ct);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Document category not found or inactive.");
            }
        }

        var storagePath = await storageService.SaveAsync(request.Content, extension, request.ReferenceType, request.ReferenceId, ct);

        var entity = new DocDocument
        {
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            CategoryId = request.CategoryId,
            OriginalFileName = request.OriginalFileName,
            StoredFileName = Path.GetFileName(storagePath),
            FileExtension = extension,
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            StoragePath = storagePath,
            Description = NormalizeText(request.Description),
            UploadedBy = currentUserId,
            UploadedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<DocDocument>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdInternalAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load uploaded document.");
    }

    public async Task<DocumentDownloadResult> DownloadAsync(int documentId, int currentUserId, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocDocument>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId, ct);

        if (entity is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        await EnsureReferenceAccessAsync(entity.ReferenceType, entity.ReferenceId, currentUserId, requireMutable: false, ct);

        var stream = await storageService.OpenReadStreamAsync(entity.StoragePath, ct);

        return new DocumentDownloadResult
        {
            Content = stream,
            ContentType = entity.ContentType,
            FileName = entity.OriginalFileName
        };
    }

    public async Task<bool> DeleteAsync(int documentId, int currentUserId, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocDocument>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == documentId, ct);

        if (entity is null)
        {
            return false;
        }

        await EnsureReferenceAccessAsync(entity.ReferenceType, entity.ReferenceId, currentUserId, requireMutable: true, ct);

        unitOfWork.Repository<DocDocument>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        await storageService.DeleteAsync(entity.StoragePath, ct);

        return true;
    }

    public async Task<IReadOnlyList<DocumentCategoryDto>> GetCategoryOptionsAsync(CancellationToken ct = default)
    {
        return await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DocumentCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Module = x.Module,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<DocumentCategoryDto>> GetCategoriesPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DocumentCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Module = x.Module,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return PagedResult<DocumentCategoryDto>.Create(items, total, page, pageSize);
    }

    public async Task<DocumentCategoryDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DocumentCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Module = x.Module,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DocumentCategoryDto> CreateCategoryAsync(DocumentCategoryDto request, CancellationToken ct = default)
    {
        var exists = await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Code == request.Code, ct);

        if (exists)
        {
            throw new InvalidOperationException("Document category code already exists.");
        }

        var entity = new DocDocumentCategory
        {
            Code = request.Code,
            Name = request.Name,
            Module = NormalizeText(request.Module),
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<DocDocumentCategory>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetCategoryByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created document category.");
    }

    public async Task<DocumentCategoryDto?> UpdateCategoryAsync(int id, DocumentCategoryDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var duplicateCode = await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .AnyAsync(x => x.Id != id && x.Code == request.Code, ct);

        if (duplicateCode)
        {
            throw new InvalidOperationException("Document category code already exists.");
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Module = NormalizeText(request.Module);
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<DocDocumentCategory>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetCategoryByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocDocumentCategory>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var isUsed = await unitOfWork.Repository<DocDocument>()
            .Query()
            .AnyAsync(x => x.CategoryId == id, ct);

        if (isUsed)
        {
            throw new InvalidOperationException("Document category cannot be deleted because it is already used by uploaded documents.");
        }

        unitOfWork.Repository<DocDocumentCategory>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private async Task EnsureReferenceAccessAsync(string referenceType, int referenceId, int currentUserId, bool requireMutable, CancellationToken ct)
    {
        if (!AllowedReferenceTypes.Contains(referenceType))
        {
            throw new InvalidOperationException($"Reference type '{referenceType}' is not supported for documents.");
        }

        if (string.Equals(referenceType, "hr_leave_requests", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureLeaveRequestAccessAsync(referenceId, currentUserId, requireMutable, ct);
            return;
        }

        throw new InvalidOperationException($"Reference type '{referenceType}' has no authorization rule configured.");
    }

    private async Task EnsureLeaveRequestAccessAsync(int leaveRequestId, int currentUserId, bool requireMutable, CancellationToken ct)
    {
        var leaveRequest = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == leaveRequestId, ct);

        if (leaveRequest is null)
        {
            throw new InvalidOperationException("Leave request not found.");
        }

        var employee = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, ct);

        // Users with no linked employee profile are treated as HR/back-office staff,
        // matching the rest of the Leave module which only enforces [Authorize] and
        // has no granular per-action permission yet (see ReadMeHr.md).
        var isOwner = employee is not null && employee.Id == leaveRequest.EmployeeId;
        var isBackOffice = employee is null;

        if (!isOwner && !isBackOffice)
        {
            throw new UnauthorizedAccessException("You are not allowed to access documents for this leave request.");
        }

        if (requireMutable && leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("Documents can only be added or removed while the leave request is Pending.");
        }
    }

    private async Task<DocumentDto?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        return await unitOfWork.Repository<DocDocument>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.UploadedByUser)
            .Where(x => x.Id == id)
            .Select(x => MapDocument(x))
            .FirstOrDefaultAsync(ct);
    }

    private static DocumentDto MapDocument(DocDocument x) => new()
    {
        Id = x.Id,
        ReferenceType = x.ReferenceType,
        ReferenceId = x.ReferenceId,
        CategoryId = x.CategoryId,
        CategoryName = x.Category != null ? x.Category.Name : null,
        OriginalFileName = x.OriginalFileName,
        FileExtension = x.FileExtension,
        ContentType = x.ContentType,
        FileSizeBytes = x.FileSizeBytes,
        Description = x.Description,
        UploadedBy = x.UploadedBy,
        UploadedByName = x.UploadedByUser.FullName,
        UploadedAt = x.UploadedAt
    };

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

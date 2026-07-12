using System.Text;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Application.Options;
using ERP.Domain.Entities.Document;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace ERP.Application.Services.Document;

public sealed class DocumentService(IUnitOfWork unitOfWork, IDocumentStorageService storageService, IOptions<DocumentSettings> options) : IDocumentService
{
    private readonly DocumentSettings _settings = options.Value;

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

    public async Task<IReadOnlyList<DocumentDto>> GetByReferenceAsync(string referenceType, int referenceId, int currentUserId, CancellationToken ct = default)
    {
        await EnsureAuthorizationAsync(referenceType, referenceId, currentUserId, requireMutable: false, ct);

        return await unitOfWork.Repository<DocDocument>()
            .Query()
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .Where(x => x.ReferenceType == referenceType && x.ReferenceId == referenceId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => MapDocument(x))
            .ToListAsync(ct);
    }

    public async Task<DocumentDto> UploadAsync(UploadDocumentRequest request, int currentUserId, CancellationToken ct = default)
    {
        var config = await GetActiveConfigEntityAsync(request.ReferenceType, ct)
            ?? throw new InvalidOperationException($"Reference type '{request.ReferenceType}' is not supported for documents.");

        await ValidateFileAsync(request.OriginalFileName, request.FileSizeBytes, request.Content, config, ct);

        await EnsureAuthorizationAsync(request.ReferenceType, request.ReferenceId, currentUserId, requireMutable: true, ct);

        var existingCount = await unitOfWork.Repository<DocDocument>()
            .Query()
            .CountAsync(x => x.ReferenceType == request.ReferenceType && x.ReferenceId == request.ReferenceId, ct);

        if (existingCount >= config.MaxFileCount)
        {
            throw new InvalidOperationException($"Maximum of {config.MaxFileCount} attachment(s) allowed for this record.");
        }

        var extension = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
        var storagePath = await storageService.SaveAsync(request.Content, extension, request.ReferenceType, request.ReferenceId, ct);

        var entity = new DocDocument
        {
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
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

        await EnsureAuthorizationAsync(entity.ReferenceType, entity.ReferenceId, currentUserId, requireMutable: false, ct);

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

        await EnsureAuthorizationAsync(entity.ReferenceType, entity.ReferenceId, currentUserId, requireMutable: true, ct);

        unitOfWork.Repository<DocDocument>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        await storageService.DeleteAsync(entity.StoragePath, ct);

        return true;
    }

    public async Task<DocumentReferenceTypeConfigDto?> GetConfigAsync(string referenceType, CancellationToken ct = default)
    {
        var entity = await GetActiveConfigEntityAsync(referenceType, ct);
        return entity is null ? null : MapConfig(entity);
    }

    public async Task<PagedResult<DocumentReferenceTypeConfigDto>> GetConfigsPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.DisplayName.ToLower().Contains(search) || x.ReferenceType.ToLower().Contains(search));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapConfig(x))
            .ToListAsync(ct);

        return PagedResult<DocumentReferenceTypeConfigDto>.Create(items, total, page, pageSize);
    }

    public async Task<DocumentReferenceTypeConfigDto?> GetConfigByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapConfig(entity);
    }

    public async Task<DocumentReferenceTypeConfigDto> CreateConfigAsync(DocumentReferenceTypeConfigDto request, CancellationToken ct = default)
    {
        ValidateConfigRequest(request);

        var exists = await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ReferenceType == request.ReferenceType, ct);

        if (exists)
        {
            throw new InvalidOperationException("This reference type already has a configuration.");
        }

        var entity = new DocReferenceTypeConfig
        {
            ReferenceType = request.ReferenceType,
            DisplayName = request.DisplayName,
            IsRequired = request.IsRequired,
            MaxFileSizeBytes = request.MaxFileSizeBytes,
            MaxFileCount = request.MaxFileCount,
            AllowedExtensions = NormalizeText(request.AllowedExtensions),
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<DocReferenceTypeConfig>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapConfig(entity);
    }

    public async Task<DocumentReferenceTypeConfigDto?> UpdateConfigAsync(int id, DocumentReferenceTypeConfigDto request, CancellationToken ct = default)
    {
        ValidateConfigRequest(request);

        var entity = await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var duplicateType = await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .AnyAsync(x => x.Id != id && x.ReferenceType == request.ReferenceType, ct);

        if (duplicateType)
        {
            throw new InvalidOperationException("This reference type already has a configuration.");
        }

        entity.ReferenceType = request.ReferenceType;
        entity.DisplayName = request.DisplayName;
        entity.IsRequired = request.IsRequired;
        entity.MaxFileSizeBytes = request.MaxFileSizeBytes;
        entity.MaxFileCount = request.MaxFileCount;
        entity.AllowedExtensions = NormalizeText(request.AllowedExtensions);
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<DocReferenceTypeConfig>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return MapConfig(entity);
    }

    public async Task<bool> DeleteConfigAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasDocuments = await unitOfWork.Repository<DocDocument>()
            .Query()
            .AnyAsync(x => x.ReferenceType == entity.ReferenceType, ct);

        if (hasDocuments)
        {
            throw new InvalidOperationException("This configuration cannot be deleted because documents already use this reference type.");
        }

        unitOfWork.Repository<DocReferenceTypeConfig>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static void ValidateConfigRequest(DocumentReferenceTypeConfigDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            throw new InvalidOperationException("Reference type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new InvalidOperationException("Display name is required.");
        }

        if (request.MaxFileCount < 1)
        {
            throw new InvalidOperationException("Max file count must be at least 1.");
        }

        if (request.MaxFileSizeBytes.HasValue && request.MaxFileSizeBytes.Value <= 0)
        {
            throw new InvalidOperationException("Max file size must be greater than 0.");
        }
    }

    private async Task<DocReferenceTypeConfig?> GetActiveConfigEntityAsync(string referenceType, CancellationToken ct)
    {
        return await unitOfWork.Repository<DocReferenceTypeConfig>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReferenceType == referenceType && x.IsActive, ct);
    }

    private async Task ValidateFileAsync(string originalFileName, long fileSizeBytes, Stream content, DocReferenceTypeConfig config, CancellationToken ct)
    {
        if (fileSizeBytes <= 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        var maxSize = config.MaxFileSizeBytes ?? _settings.MaxFileSizeBytes;
        if (fileSizeBytes > maxSize)
        {
            throw new InvalidOperationException($"File must be {maxSize / (1024 * 1024)} MB or smaller.");
        }

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        var allowedExtensions = ParseAllowedExtensions(config.AllowedExtensions) ?? _settings.AllowedExtensions;
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"File type not allowed. Allowed: {string.Join(", ", allowedExtensions)}");
        }

        await ValidateFileIntegrityAsync(extension, content, ct);
    }

    private static string[]? ParseAllowedExtensions(string? commaSeparated)
    {
        if (string.IsNullOrWhiteSpace(commaSeparated))
        {
            return null;
        }

        return commaSeparated
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static async Task ValidateFileIntegrityAsync(string extension, Stream content, CancellationToken ct)
    {
        if (!content.CanSeek)
        {
            // Can't rewind a non-seekable stream to re-use it for the actual save afterward,
            // so skip the deep integrity check defensively rather than consume the stream here.
            return;
        }

        var originalPosition = content.Position;
        try
        {
            if (ImageExtensions.Contains(extension))
            {
                IImageFormat? format;
                try
                {
                    format = await Image.DetectFormatAsync(content, ct);
                }
                catch
                {
                    format = null;
                }

                if (format is null)
                {
                    throw new InvalidOperationException("The uploaded image file appears to be corrupted or is not a valid image.");
                }
            }
            else if (extension == ".pdf")
            {
                var header = new byte[5];
                var read = await content.ReadAsync(header.AsMemory(0, 5), ct);
                if (read < 5 || Encoding.ASCII.GetString(header) != "%PDF-")
                {
                    throw new InvalidOperationException("The uploaded PDF file appears to be corrupted or is not a valid PDF.");
                }
            }
            else if (extension == ".docx")
            {
                try
                {
                    using var archive = new System.IO.Compression.ZipArchive(content, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
                    if (archive.GetEntry("[Content_Types].xml") is null)
                    {
                        throw new InvalidOperationException("The uploaded DOCX file appears to be corrupted or is not a valid Word document.");
                    }
                }
                catch (InvalidDataException)
                {
                    throw new InvalidOperationException("The uploaded DOCX file appears to be corrupted or is not a valid Word document.");
                }
            }
        }
        finally
        {
            content.Position = originalPosition;
        }
    }

    private async Task EnsureAuthorizationAsync(string referenceType, int referenceId, int currentUserId, bool requireMutable, CancellationToken ct)
    {
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
        OriginalFileName = x.OriginalFileName,
        FileExtension = x.FileExtension,
        ContentType = x.ContentType,
        FileSizeBytes = x.FileSizeBytes,
        Description = x.Description,
        UploadedBy = x.UploadedBy,
        UploadedByName = x.UploadedByUser.FullName,
        UploadedAt = x.UploadedAt
    };

    private static DocumentReferenceTypeConfigDto MapConfig(DocReferenceTypeConfig x) => new()
    {
        Id = x.Id,
        ReferenceType = x.ReferenceType,
        DisplayName = x.DisplayName,
        IsRequired = x.IsRequired,
        MaxFileSizeBytes = x.MaxFileSizeBytes,
        MaxFileCount = x.MaxFileCount,
        AllowedExtensions = x.AllowedExtensions,
        IsActive = x.IsActive
    };

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

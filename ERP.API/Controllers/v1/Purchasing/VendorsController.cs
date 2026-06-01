using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Purchasing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Purchasing;

[Route("api/v1/purchasing/vendors")]
public sealed class VendorsController(AppDbContext dbContext) : PurchasingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PurchasingVendorPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinVendors
            .AsNoTracking()
            .Include(x => x.VendorCategory)
            .Include(x => x.BuyerGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
                (x.TaxId != null && x.TaxId.ToLower().Contains(search)) ||
                (x.Email != null && x.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.VendorCategoryId.HasValue)
        {
            query = query.Where(x => x.VendorCategoryId == request.VendorCategoryId.Value);
        }

        if (request.BuyerGroupId.HasValue)
        {
            query = query.Where(x => x.BuyerGroupId == request.BuyerGroupId.Value);
        }

        if (request.IsApprovedVendor.HasValue)
        {
            query = query.Where(x => x.IsApprovedVendor == request.IsApprovedVendor.Value);
        }

        if (request.PerformanceScoreFrom.HasValue)
        {
            query = query.Where(x => x.PerformanceScore.HasValue && x.PerformanceScore.Value >= request.PerformanceScoreFrom.Value);
        }

        if (request.PerformanceScoreTo.HasValue)
        {
            query = query.Where(x => x.PerformanceScore.HasValue && x.PerformanceScore.Value <= request.PerformanceScoreTo.Value);
        }

        if (request.PaymentTermsFrom.HasValue)
        {
            query = query.Where(x => x.PaymentTermsDays >= request.PaymentTermsFrom.Value);
        }

        if (request.PaymentTermsTo.HasValue)
        {
            query = query.Where(x => x.PaymentTermsDays <= request.PaymentTermsTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "vendorcategoryname" => isDesc ? query.OrderByDescending(x => x.VendorCategory!.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.VendorCategory!.Name).ThenBy(x => x.Code),
            "buyergroupname" => isDesc ? query.OrderByDescending(x => x.BuyerGroup!.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.BuyerGroup!.Name).ThenBy(x => x.Code),
            "performancescore" => isDesc ? query.OrderByDescending(x => x.PerformanceScore).ThenByDescending(x => x.Code) : query.OrderBy(x => x.PerformanceScore).ThenBy(x => x.Code),
            "paymenttermsdays" => isDesc ? query.OrderByDescending(x => x.PaymentTermsDays).ThenByDescending(x => x.Code) : query.OrderBy(x => x.PaymentTermsDays).ThenBy(x => x.Code),
            "isapprovedvendor" => isDesc ? query.OrderByDescending(x => x.IsApprovedVendor).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsApprovedVendor).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<PurchasingVendorDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinVendors
            .AsNoTracking()
            .Include(x => x.VendorCategory)
            .Include(x => x.BuyerGroup)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id:int}/set-approved")]
    public async Task<IActionResult> SetApproved(int id, [FromBody] SetApprovedVendorRequest request, CancellationToken ct)
    {
        var entity = await dbContext.FinVendors.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        entity.IsApprovedVendor = request.IsApproved;
        entity.ApprovedDate = request.IsApproved
            ? (request.ApprovedDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date))
            : null;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.FinVendors
            .AsNoTracking()
            .Include(x => x.VendorCategory)
            .Include(x => x.BuyerGroup)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(updated);
    }

    private static PurchasingVendorDto MapDto(ERP.Domain.Entities.Finance.FinVendor entity)
    {
        return new PurchasingVendorDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            TaxId = entity.TaxId,
            Address = entity.Address,
            Phone = entity.Phone,
            Email = entity.Email,
            ContactPerson = entity.ContactPerson,
            PaymentTermsDays = entity.PaymentTermsDays,
            VendorCategoryId = entity.VendorCategoryId,
            VendorCategoryCode = entity.VendorCategory?.Code,
            VendorCategoryName = entity.VendorCategory?.Name,
            BuyerGroupId = entity.BuyerGroupId,
            BuyerGroupCode = entity.BuyerGroup?.Code,
            BuyerGroupName = entity.BuyerGroup?.Name,
            IsApprovedVendor = entity.IsApprovedVendor,
            ApprovedDate = entity.ApprovedDate,
            LeadTimeDays = entity.LeadTimeDays,
            PerformanceScore = entity.PerformanceScore,
            IsActive = entity.IsActive,
            PoHistoryCount = 0,
            PoHistoryAmount = 0m,
            ReturnCount = 0
        };
    }

    public sealed class SetApprovedVendorRequest
    {
        public bool IsApproved { get; set; }
        public DateOnly? ApprovedDate { get; set; }
    }
}


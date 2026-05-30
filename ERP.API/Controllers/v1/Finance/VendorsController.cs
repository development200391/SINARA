using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/vendors")]
public sealed class VendorsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] VendorPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinVendors
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.TaxId != null && x.TaxId.ToLower().Contains(search)) ||
                (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
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

        if (!string.IsNullOrWhiteSpace(request.TaxId))
        {
            var taxId = request.TaxId.Trim().ToLowerInvariant();
            query = query.Where(x => x.TaxId != null && x.TaxId.ToLower().Contains(taxId));
        }

        if (!string.IsNullOrWhiteSpace(request.ContactPerson))
        {
            var contactPerson = request.ContactPerson.Trim().ToLowerInvariant();
            query = query.Where(x => x.ContactPerson != null && x.ContactPerson.ToLower().Contains(contactPerson));
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
            "taxid" => isDesc ? query.OrderByDescending(x => x.TaxId).ThenByDescending(x => x.Code) : query.OrderBy(x => x.TaxId).ThenBy(x => x.Code),
            "contactperson" => isDesc ? query.OrderByDescending(x => x.ContactPerson).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ContactPerson).ThenBy(x => x.Code),
            "paymenttermsdays" => isDesc ? query.OrderByDescending(x => x.PaymentTermsDays).ThenByDescending(x => x.Code) : query.OrderBy(x => x.PaymentTermsDays).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(VendorProjection)
            .ToListAsync(ct);

        return Ok(PagedResult<VendorDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinVendors
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(VendorProjection)
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VendorDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Vendor code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Vendor name is required.");

            if (request.PaymentTermsDays < 0)
            {
                return BadRequest(new { message = "Payment terms cannot be negative." });
            }

            var defaultAccountId = request.DefaultAccountId is > 0 ? request.DefaultAccountId : null;
            if (defaultAccountId.HasValue)
            {
                var accountExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == defaultAccountId.Value, ct);
                if (!accountExists)
                {
                    return BadRequest(new { message = "Default account not found." });
                }
            }

            var defaultTaxCodeId = request.DefaultTaxCodeId is > 0 ? request.DefaultTaxCodeId : null;
            if (defaultTaxCodeId.HasValue)
            {
                var taxCodeExists = await dbContext.FinTaxCodes.AnyAsync(x => x.Id == defaultTaxCodeId.Value, ct);
                if (!taxCodeExists)
                {
                    return BadRequest(new { message = "Default tax code not found." });
                }
            }

            var duplicate = await dbContext.FinVendors
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Vendor code already exists." });
            }

            var entity = new FinVendor
            {
                Code = normalizedCode,
                Name = normalizedName,
                TaxId = NormalizeOptional(request.TaxId),
                Address = NormalizeOptional(request.Address),
                Phone = NormalizeOptional(request.Phone),
                Email = NormalizeOptional(request.Email),
                ContactPerson = NormalizeOptional(request.ContactPerson),
                PaymentTermsDays = request.PaymentTermsDays,
                DefaultAccountId = defaultAccountId,
                DefaultTaxCodeId = defaultTaxCodeId,
                BankName = NormalizeOptional(request.BankName),
                BankAccountNo = NormalizeOptional(request.BankAccountNo),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FinVendors.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinVendors
                .AsNoTracking()
                .Where(x => x.Id == entity.Id)
                .Select(VendorProjection)
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VendorDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinVendors.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Vendor code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Vendor name is required.");

            if (request.PaymentTermsDays < 0)
            {
                return BadRequest(new { message = "Payment terms cannot be negative." });
            }

            var duplicate = await dbContext.FinVendors
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode && x.Id != id, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Vendor code already exists." });
            }

            var defaultAccountId = request.DefaultAccountId is > 0 ? request.DefaultAccountId : null;
            if (defaultAccountId.HasValue)
            {
                var accountExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == defaultAccountId.Value, ct);
                if (!accountExists)
                {
                    return BadRequest(new { message = "Default account not found." });
                }
            }

            var defaultTaxCodeId = request.DefaultTaxCodeId is > 0 ? request.DefaultTaxCodeId : null;
            if (defaultTaxCodeId.HasValue)
            {
                var taxCodeExists = await dbContext.FinTaxCodes.AnyAsync(x => x.Id == defaultTaxCodeId.Value, ct);
                if (!taxCodeExists)
                {
                    return BadRequest(new { message = "Default tax code not found." });
                }
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.TaxId = NormalizeOptional(request.TaxId);
            entity.Address = NormalizeOptional(request.Address);
            entity.Phone = NormalizeOptional(request.Phone);
            entity.Email = NormalizeOptional(request.Email);
            entity.ContactPerson = NormalizeOptional(request.ContactPerson);
            entity.PaymentTermsDays = request.PaymentTermsDays;
            entity.DefaultAccountId = defaultAccountId;
            entity.DefaultTaxCodeId = defaultTaxCodeId;
            entity.BankName = NormalizeOptional(request.BankName);
            entity.BankAccountNo = NormalizeOptional(request.BankAccountNo);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinVendors
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(VendorProjection)
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinVendors.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasInvoices = await dbContext.FinApInvoices.AnyAsync(x => x.VendorId == id, ct);
        var hasPayments = await dbContext.FinApPayments.AnyAsync(x => x.VendorId == id, ct);
        if (hasInvoices || hasPayments)
        {
            return BadRequest(new { message = "Vendor already used in AP transactions and cannot be deleted." });
        }

        dbContext.FinVendors.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static readonly Expression<Func<FinVendor, VendorDto>> VendorProjection = entity => new VendorDto
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
        DefaultAccountId = entity.DefaultAccountId,
        DefaultAccountCode = entity.DefaultAccount != null ? entity.DefaultAccount.Code : null,
        DefaultAccountName = entity.DefaultAccount != null ? entity.DefaultAccount.Name : null,
        DefaultTaxCodeId = entity.DefaultTaxCodeId,
        DefaultTaxCodeCode = entity.DefaultTaxCode != null ? entity.DefaultTaxCode.Code : null,
        DefaultTaxCodeName = entity.DefaultTaxCode != null ? entity.DefaultTaxCode.Name : null,
        BankName = entity.BankName,
        BankAccountNo = entity.BankAccountNo,
        IsActive = entity.IsActive
    };

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

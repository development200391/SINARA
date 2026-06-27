using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Entities.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/price-lists")]
public sealed class PriceListsController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesPriceListPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.SalPriceLists
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.CurrencyCode.ToLower().Contains(search) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
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

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            var currencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CurrencyCode == currencyCode);
        }

        if (request.ValidFromFrom.HasValue)
        {
            query = query.Where(x => x.ValidFrom >= request.ValidFromFrom.Value);
        }

        if (request.ValidFromTo.HasValue)
        {
            query = query.Where(x => x.ValidFrom <= request.ValidFromTo.Value);
        }

        if (request.ValidToFrom.HasValue)
        {
            query = query.Where(x => x.ValidTo.HasValue && x.ValidTo.Value >= request.ValidToFrom.Value);
        }

        if (request.ValidToTo.HasValue)
        {
            query = query.Where(x => x.ValidTo.HasValue && x.ValidTo.Value <= request.ValidToTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "type" => isDesc ? query.OrderByDescending(x => x.Type).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Type).ThenBy(x => x.Code),
            "currencycode" => isDesc ? query.OrderByDescending(x => x.CurrencyCode).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CurrencyCode).ThenBy(x => x.Code),
            "validfrom" => isDesc ? query.OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ValidFrom).ThenBy(x => x.Code),
            "validto" => isDesc ? query.OrderByDescending(x => x.ValidTo).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ValidTo).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapPriceListDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<SalesPriceListDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.SalPriceLists
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new SalesOptionDto
            {
                Id = x.Id,
                Label = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.SalPriceLists
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapPriceListDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesPriceListDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Price list code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Price list name is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            var validation = await ValidatePriceListAsync(normalizedCode, normalizedCurrencyCode, request.ValidFrom, request.ValidTo, null, ct);
            if (validation is not null)
            {
                return validation;
            }

            var entity = new SalPriceList
            {
                Code = normalizedCode,
                Name = normalizedName,
                Type = request.Type,
                CurrencyCode = normalizedCurrencyCode,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.SalPriceLists.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.SalPriceLists
                .AsNoTracking()
                .Where(x => x.Id == entity.Id)
                .Select(x => MapPriceListDto(x))
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesPriceListDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.SalPriceLists.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Price list code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Price list name is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            var validation = await ValidatePriceListAsync(normalizedCode, normalizedCurrencyCode, request.ValidFrom, request.ValidTo, id, ct);
            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Type = request.Type;
            entity.CurrencyCode = normalizedCurrencyCode;
            entity.ValidFrom = request.ValidFrom;
            entity.ValidTo = request.ValidTo;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.SalPriceLists
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => MapPriceListDto(x))
                .FirstAsync(ct);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.SalPriceLists.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var usedByCategory = await dbContext.SalCustomerCategories.AnyAsync(x => x.DefaultPriceListId == id, ct);
        var usedByCustomer = await dbContext.FinCustomers.AnyAsync(x => x.PriceListId == id, ct);
        if (usedByCategory || usedByCustomer)
        {
            return BadRequest(new { message = "Price list is already used by category or customer." });
        }

        dbContext.SalPriceLists.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("{priceListId:int}/items")]
    public async Task<IActionResult> GetItems(int priceListId, [FromQuery] SalesPriceListItemPagedRequest request, CancellationToken ct)
    {
        var priceListExists = await dbContext.SalPriceLists.AnyAsync(x => x.Id == priceListId, ct);
        if (!priceListExists)
        {
            return NotFound();
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.SalPriceListItems
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Uom)
            .Where(x => x.PriceListId == priceListId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.Uom.Code.ToLower().Contains(search) ||
                x.Uom.Name.ToLower().Contains(search));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.UomId.HasValue)
        {
            query = query.Where(x => x.UomId == request.UomId.Value);
        }

        if (request.MinQtyFrom.HasValue)
        {
            query = query.Where(x => x.MinQty >= request.MinQtyFrom.Value);
        }

        if (request.MinQtyTo.HasValue)
        {
            query = query.Where(x => x.MinQty <= request.MinQtyTo.Value);
        }

        if (request.UnitPriceFrom.HasValue)
        {
            query = query.Where(x => x.UnitPrice >= request.UnitPriceFrom.Value);
        }

        if (request.UnitPriceTo.HasValue)
        {
            query = query.Where(x => x.UnitPrice <= request.UnitPriceTo.Value);
        }

        if (request.DiscountPctFrom.HasValue)
        {
            query = query.Where(x => x.DiscountPct >= request.DiscountPctFrom.Value);
        }

        if (request.DiscountPctTo.HasValue)
        {
            query = query.Where(x => x.DiscountPct <= request.DiscountPctTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode).ThenByDescending(x => x.MinQty) : query.OrderBy(x => x.Item.ItemCode).ThenBy(x => x.MinQty),
            "itemname" => isDesc ? query.OrderByDescending(x => x.Item.Name).ThenByDescending(x => x.MinQty) : query.OrderBy(x => x.Item.Name).ThenBy(x => x.MinQty),
            "uomcode" => isDesc ? query.OrderByDescending(x => x.Uom.Code).ThenByDescending(x => x.MinQty) : query.OrderBy(x => x.Uom.Code).ThenBy(x => x.MinQty),
            "minqty" => isDesc ? query.OrderByDescending(x => x.MinQty).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.MinQty).ThenBy(x => x.Item.ItemCode),
            "unitprice" => isDesc ? query.OrderByDescending(x => x.UnitPrice).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.UnitPrice).ThenBy(x => x.Item.ItemCode),
            "discountpct" => isDesc ? query.OrderByDescending(x => x.DiscountPct).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.DiscountPct).ThenBy(x => x.Item.ItemCode),
            _ => isDesc ? query.OrderByDescending(x => x.Item.ItemCode).ThenByDescending(x => x.MinQty) : query.OrderBy(x => x.Item.ItemCode).ThenBy(x => x.MinQty)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapPriceListItemDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<SalesPriceListItemDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{priceListId:int}/items/{id:int}")]
    public async Task<IActionResult> GetItemById(int priceListId, int id, CancellationToken ct)
    {
        var item = await dbContext.SalPriceListItems
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Uom)
            .Where(x => x.PriceListId == priceListId && x.Id == id)
            .Select(x => MapPriceListItemDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{priceListId:int}/items")]
    public async Task<IActionResult> CreateItem(int priceListId, [FromBody] SalesPriceListItemDto request, CancellationToken ct)
    {
        var validation = await ValidatePriceListItemAsync(priceListId, request, null, ct);
        if (validation is not null)
        {
            return validation;
        }

        var entity = new SalPriceListItem
        {
            PriceListId = priceListId,
            ItemId = request.ItemId,
            UomId = request.UomId,
            MinQty = decimal.Round(request.MinQty, 4, MidpointRounding.AwayFromZero),
            UnitPrice = decimal.Round(request.UnitPrice, 4, MidpointRounding.AwayFromZero),
            DiscountPct = decimal.Round(request.DiscountPct, 2, MidpointRounding.AwayFromZero),
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
        };

        dbContext.SalPriceListItems.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        var created = await dbContext.SalPriceListItems
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Uom)
            .Where(x => x.Id == entity.Id)
            .Select(x => MapPriceListItemDto(x))
            .FirstAsync(ct);

        return Ok(created);
    }

    [HttpPut("{priceListId:int}/items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int priceListId, int id, [FromBody] SalesPriceListItemDto request, CancellationToken ct)
    {
        var entity = await dbContext.SalPriceListItems
            .FirstOrDefaultAsync(x => x.PriceListId == priceListId && x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        var validation = await ValidatePriceListItemAsync(priceListId, request, id, ct);
        if (validation is not null)
        {
            return validation;
        }

        entity.ItemId = request.ItemId;
        entity.UomId = request.UomId;
        entity.MinQty = decimal.Round(request.MinQty, 4, MidpointRounding.AwayFromZero);
        entity.UnitPrice = decimal.Round(request.UnitPrice, 4, MidpointRounding.AwayFromZero);
        entity.DiscountPct = decimal.Round(request.DiscountPct, 2, MidpointRounding.AwayFromZero);
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.SalPriceListItems
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Uom)
            .Where(x => x.PriceListId == priceListId && x.Id == id)
            .Select(x => MapPriceListItemDto(x))
            .FirstAsync(ct);

        return Ok(updated);
    }

    [HttpDelete("{priceListId:int}/items/{id:int}")]
    public async Task<IActionResult> DeleteItem(int priceListId, int id, CancellationToken ct)
    {
        var entity = await dbContext.SalPriceListItems
            .FirstOrDefaultAsync(x => x.PriceListId == priceListId && x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        dbContext.SalPriceListItems.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidatePriceListAsync(string code, string currencyCode, DateOnly validFrom, DateOnly? validTo, int? id, CancellationToken ct)
    {
        if (validTo.HasValue && validTo.Value < validFrom)
        {
            return BadRequest(new { message = "Valid-to date must be greater than or equal to valid-from date." });
        }

        var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == currencyCode, ct);
        if (!currencyExists)
        {
            return BadRequest(new { message = "Currency code not found." });
        }

        var duplicate = await dbContext.SalPriceLists
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Price list code already exists." });
        }

        return null;
    }

    private async Task<IActionResult?> ValidatePriceListItemAsync(int priceListId, SalesPriceListItemDto request, int? id, CancellationToken ct)
    {
        var priceListExists = await dbContext.SalPriceLists.AnyAsync(x => x.Id == priceListId, ct);
        if (!priceListExists)
        {
            return NotFound();
        }

        if (request.ItemId <= 0)
        {
            return BadRequest(new { message = "Item is required." });
        }

        if (request.UomId <= 0)
        {
            return BadRequest(new { message = "Unit of measure is required." });
        }

        if (request.MinQty <= 0)
        {
            return BadRequest(new { message = "Minimum quantity must be greater than 0." });
        }

        if (request.UnitPrice < 0)
        {
            return BadRequest(new { message = "Unit price cannot be negative." });
        }

        if (request.DiscountPct < 0 || request.DiscountPct > 100)
        {
            return BadRequest(new { message = "Discount percentage must be in range 0..100." });
        }

        var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == request.ItemId, ct);
        if (!itemExists)
        {
            return BadRequest(new { message = "Item not found." });
        }

        var uomExists = await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == request.UomId, ct);
        if (!uomExists)
        {
            return BadRequest(new { message = "Unit of measure not found." });
        }

        var roundedMinQty = decimal.Round(request.MinQty, 4, MidpointRounding.AwayFromZero);
        var duplicate = await dbContext.SalPriceListItems
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (id ?? 0)
                && x.PriceListId == priceListId
                && x.ItemId == request.ItemId
                && x.UomId == request.UomId
                && x.MinQty == roundedMinQty,
                ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Item tier already exists." });
        }

        return null;
    }

    private static SalesPriceListDto MapPriceListDto(SalPriceList entity)
    {
        return new SalesPriceListDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            CurrencyCode = entity.CurrencyCode,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    private static SalesPriceListItemDto MapPriceListItemDto(SalPriceListItem entity)
    {
        return new SalesPriceListItemDto
        {
            Id = entity.Id,
            PriceListId = entity.PriceListId,
            ItemId = entity.ItemId,
            ItemCode = entity.Item.ItemCode,
            ItemName = entity.Item.Name,
            UomId = entity.UomId,
            UomCode = entity.Uom.Code,
            UomName = entity.Uom.Name,
            MinQty = entity.MinQty,
            UnitPrice = entity.UnitPrice,
            DiscountPct = entity.DiscountPct
        };
    }

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


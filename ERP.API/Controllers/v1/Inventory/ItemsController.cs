using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/items")]
public sealed class ItemsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ItemPagedRequest request, CancellationToken ct)
    {
        var result = await GetPagedInternalAsync(request, lowStockOnly: false, ct);
        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] ItemPagedRequest request, CancellationToken ct)
    {
        var result = await GetPagedInternalAsync(request, lowStockOnly: true, ct);
        return Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.InvItems
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status == ItemStatus.Active)
            .OrderBy(x => x.ItemCode)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.ItemCode + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.InvItems
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.BaseUom)
            .Include(x => x.PurchaseUom)
            .Include(x => x.InventoryAccount)
            .Include(x => x.CogsAccount)
            .Include(x => x.AdjustmentAccount)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                Item = x,
                QtyAvailable = x.StockBalances.Sum(sb => (decimal?)sb.QtyAvailable) ?? 0m
            })
            .FirstOrDefaultAsync(ct);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(MapDto(item.Item, item.QtyAvailable));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemDto request, CancellationToken ct)
    {
        try
        {
            await ValidateDependenciesAsync(request, null, ct);

            ValidateNumericInput(request);

            var normalizedCode = string.IsNullOrWhiteSpace(request.ItemCode)
                ? await GenerateItemCodeAsync(ct)
                : request.ItemCode.Trim().ToUpperInvariant();

            var duplicateCode = await dbContext.InvItems
                .IgnoreQueryFilters()
                .AnyAsync(x => x.ItemCode == normalizedCode, ct);
            if (duplicateCode)
            {
                return BadRequest(new { message = "Item code already exists." });
            }

            var normalizedSku = NormalizeOptional(request.Sku);
            if (!string.IsNullOrWhiteSpace(normalizedSku))
            {
                var duplicateSku = await dbContext.InvItems
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Sku != null && x.Sku.ToLower() == normalizedSku.ToLower(), ct);
                if (duplicateSku)
                {
                    return BadRequest(new { message = "SKU already exists." });
                }
            }

            var entity = new InvItem
            {
                ItemCode = normalizedCode,
                Sku = normalizedSku,
                Name = NormalizeRequired(request.Name, "Item name is required."),
                Description = NormalizeOptional(request.Description),
                CategoryId = request.CategoryId,
                BrandId = request.BrandId is > 0 ? request.BrandId : null,
                Type = request.Type,
                BaseUomId = request.BaseUomId,
                PurchaseUomId = request.PurchaseUomId is > 0 ? request.PurchaseUomId : null,
                Status = request.Status,
                ValuationMethod = request.ValuationMethod,
                LastPurchasePrice = request.LastPurchasePrice,
                AvgCost = request.AvgCost,
                MinStock = request.MinStock,
                MaxStock = request.MaxStock,
                ReorderPoint = request.ReorderPoint,
                LeadTimeDays = request.LeadTimeDays,
                InventoryAccountId = request.InventoryAccountId is > 0 ? request.InventoryAccountId : null,
                CogsAccountId = request.CogsAccountId is > 0 ? request.CogsAccountId : null,
                AdjustmentAccountId = request.AdjustmentAccountId is > 0 ? request.AdjustmentAccountId : null,
                Notes = NormalizeOptional(request.Notes),
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvItems.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.BaseUom)
                .Include(x => x.PurchaseUom)
                .Include(x => x.InventoryAccount)
                .Include(x => x.CogsAccount)
                .Include(x => x.AdjustmentAccount)
                .Where(x => x.Id == entity.Id)
                .Select(x => new
                {
                    Item = x,
                    QtyAvailable = x.StockBalances.Sum(sb => (decimal?)sb.QtyAvailable) ?? 0m
                })
                .FirstAsync(ct);

            return Ok(MapDto(created.Item, created.QtyAvailable));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvItems.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            await ValidateDependenciesAsync(request, id, ct);
            ValidateNumericInput(request);

            var normalizedSku = NormalizeOptional(request.Sku);
            if (!string.IsNullOrWhiteSpace(normalizedSku))
            {
                var duplicateSku = await dbContext.InvItems
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Id != id && x.Sku != null && x.Sku.ToLower() == normalizedSku.ToLower(), ct);
                if (duplicateSku)
                {
                    return BadRequest(new { message = "SKU already exists." });
                }
            }

            entity.Sku = normalizedSku;
            entity.Name = NormalizeRequired(request.Name, "Item name is required.");
            entity.Description = NormalizeOptional(request.Description);
            entity.CategoryId = request.CategoryId;
            entity.BrandId = request.BrandId is > 0 ? request.BrandId : null;
            entity.Type = request.Type;
            entity.BaseUomId = request.BaseUomId;
            entity.PurchaseUomId = request.PurchaseUomId is > 0 ? request.PurchaseUomId : null;
            entity.Status = request.Status;
            entity.ValuationMethod = request.ValuationMethod;
            entity.LastPurchasePrice = request.LastPurchasePrice;
            entity.AvgCost = request.AvgCost;
            entity.MinStock = request.MinStock;
            entity.MaxStock = request.MaxStock;
            entity.ReorderPoint = request.ReorderPoint;
            entity.LeadTimeDays = request.LeadTimeDays;
            entity.InventoryAccountId = request.InventoryAccountId is > 0 ? request.InventoryAccountId : null;
            entity.CogsAccountId = request.CogsAccountId is > 0 ? request.CogsAccountId : null;
            entity.AdjustmentAccountId = request.AdjustmentAccountId is > 0 ? request.AdjustmentAccountId : null;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.BaseUom)
                .Include(x => x.PurchaseUom)
                .Include(x => x.InventoryAccount)
                .Include(x => x.CogsAccount)
                .Include(x => x.AdjustmentAccount)
                .Where(x => x.Id == entity.Id)
                .Select(x => new
                {
                    Item = x,
                    QtyAvailable = x.StockBalances.Sum(sb => (decimal?)sb.QtyAvailable) ?? 0m
                })
                .FirstAsync(ct);

            return Ok(MapDto(updated.Item, updated.QtyAvailable));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasConversions = await dbContext.InvItemUnitConversions.AnyAsync(x => x.ItemId == id, ct);
        if (hasConversions)
        {
            return BadRequest(new { message = "Item cannot be deleted because it has unit conversions." });
        }

        var hasStock = await dbContext.InvStockBalances.AnyAsync(x => x.ItemId == id, ct);
        if (hasStock)
        {
            return BadRequest(new { message = "Item cannot be deleted because it has stock balances." });
        }

        dbContext.InvItems.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<PagedResult<ItemDto>> GetPagedInternalAsync(ItemPagedRequest request, bool lowStockOnly, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var baseQuery = dbContext.InvItems
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.BaseUom)
            .Include(x => x.PurchaseUom)
            .Include(x => x.InventoryAccount)
            .Include(x => x.CogsAccount)
            .Include(x => x.AdjustmentAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x =>
                x.ItemCode.ToLower().Contains(search) ||
                (x.Sku != null && x.Sku.ToLower().Contains(search)) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.ItemCode))
        {
            var itemCode = request.ItemCode.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.ItemCode.ToLower().Contains(itemCode));
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var sku = request.Sku.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Sku != null && x.Sku.ToLower().Contains(sku));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.BrandId == request.BrandId.Value);
        }

        if (request.Type.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Type == request.Type.Value);
        }

        if (request.Status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Status == request.Status.Value);
        }

        if (request.IsActive.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.MinStockFrom.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.MinStock >= request.MinStockFrom.Value);
        }

        if (request.MinStockTo.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.MinStock <= request.MinStockTo.Value);
        }

        if (request.ReorderPointFrom.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.ReorderPoint >= request.ReorderPointFrom.Value);
        }

        if (request.ReorderPointTo.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.ReorderPoint <= request.ReorderPointTo.Value);
        }

        var query = baseQuery.Select(x => new
        {
            Item = x,
            QtyAvailable = x.StockBalances.Sum(sb => (decimal?)sb.QtyAvailable) ?? 0m
        });

        if (lowStockOnly)
        {
            query = query.Where(x => x.QtyAvailable <= x.Item.MinStock);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "sku" => isDesc ? query.OrderByDescending(x => x.Item.Sku) : query.OrderBy(x => x.Item.Sku),
            "name" => isDesc ? query.OrderByDescending(x => x.Item.Name) : query.OrderBy(x => x.Item.Name),
            "categoryname" => isDesc ? query.OrderByDescending(x => x.Item.Category.Name).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.Category.Name).ThenBy(x => x.Item.ItemCode),
            "status" => isDesc ? query.OrderByDescending(x => x.Item.Status).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.Status).ThenBy(x => x.Item.ItemCode),
            "type" => isDesc ? query.OrderByDescending(x => x.Item.Type).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.Type).ThenBy(x => x.Item.ItemCode),
            "minstock" => isDesc ? query.OrderByDescending(x => x.Item.MinStock).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.MinStock).ThenBy(x => x.Item.ItemCode),
            "reorderpoint" => isDesc ? query.OrderByDescending(x => x.Item.ReorderPoint).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ReorderPoint).ThenBy(x => x.Item.ItemCode),
            "qtyavailable" => isDesc ? query.OrderByDescending(x => x.QtyAvailable).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.QtyAvailable).ThenBy(x => x.Item.ItemCode),
            "isactive" => isDesc ? query.OrderByDescending(x => x.Item.IsActive).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.IsActive).ThenBy(x => x.Item.ItemCode),
            _ => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x.Item, x.QtyAvailable))
            .ToListAsync(ct);

        return PagedResult<ItemDto>.Create(items, totalCount, page, pageSize);
    }

    private static ItemDto MapDto(InvItem item, decimal qtyAvailable)
    {
        return new ItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            Sku = item.Sku,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name ?? string.Empty,
            BrandId = item.BrandId,
            BrandName = item.Brand?.Name,
            Type = item.Type,
            BaseUomId = item.BaseUomId,
            BaseUomCode = item.BaseUom?.Code ?? string.Empty,
            PurchaseUomId = item.PurchaseUomId,
            PurchaseUomCode = item.PurchaseUom?.Code,
            Status = item.Status,
            ValuationMethod = item.ValuationMethod,
            LastPurchasePrice = item.LastPurchasePrice,
            AvgCost = item.AvgCost,
            MinStock = item.MinStock,
            MaxStock = item.MaxStock,
            ReorderPoint = item.ReorderPoint,
            LeadTimeDays = item.LeadTimeDays,
            InventoryAccountId = item.InventoryAccountId,
            InventoryAccountCode = item.InventoryAccount?.Code,
            CogsAccountId = item.CogsAccountId,
            CogsAccountCode = item.CogsAccount?.Code,
            AdjustmentAccountId = item.AdjustmentAccountId,
            AdjustmentAccountCode = item.AdjustmentAccount?.Code,
            Notes = item.Notes,
            IsActive = item.IsActive,
            QtyAvailable = qtyAvailable
        };
    }

    private async Task ValidateDependenciesAsync(ItemDto request, int? currentId, CancellationToken ct)
    {
        if (request.CategoryId <= 0)
        {
            throw new InvalidOperationException("Category is required.");
        }

        if (!await dbContext.InvItemCategories.AnyAsync(x => x.Id == request.CategoryId, ct))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (request.BaseUomId <= 0)
        {
            throw new InvalidOperationException("Base UOM is required.");
        }

        if (!await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == request.BaseUomId, ct))
        {
            throw new InvalidOperationException("Base UOM not found.");
        }

        if (request.BrandId is > 0 && !await dbContext.InvBrands.AnyAsync(x => x.Id == request.BrandId.Value, ct))
        {
            throw new InvalidOperationException("Brand not found.");
        }

        if (request.PurchaseUomId is > 0 && !await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == request.PurchaseUomId.Value, ct))
        {
            throw new InvalidOperationException("Purchase UOM not found.");
        }

        if (request.InventoryAccountId is > 0 && !await dbContext.FinAccounts.AnyAsync(x => x.Id == request.InventoryAccountId.Value, ct))
        {
            throw new InvalidOperationException("Inventory account not found.");
        }

        if (request.CogsAccountId is > 0 && !await dbContext.FinAccounts.AnyAsync(x => x.Id == request.CogsAccountId.Value, ct))
        {
            throw new InvalidOperationException("COGS account not found.");
        }

        if (request.AdjustmentAccountId is > 0 && !await dbContext.FinAccounts.AnyAsync(x => x.Id == request.AdjustmentAccountId.Value, ct))
        {
            throw new InvalidOperationException("Adjustment account not found.");
        }

        if (currentId.HasValue && !string.IsNullOrWhiteSpace(request.ItemCode))
        {
            var normalizedCode = request.ItemCode.Trim().ToUpperInvariant();
            var duplicateCode = await dbContext.InvItems
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != currentId.Value && x.ItemCode == normalizedCode, ct);
            if (duplicateCode)
            {
                throw new InvalidOperationException("Item code already exists.");
            }
        }
    }

    private static void ValidateNumericInput(ItemDto request)
    {
        if (request.LastPurchasePrice.HasValue && request.LastPurchasePrice.Value < 0)
        {
            throw new InvalidOperationException("Last purchase price cannot be negative.");
        }

        if (request.AvgCost < 0 || request.MinStock < 0 || request.MaxStock < 0 || request.ReorderPoint < 0)
        {
            throw new InvalidOperationException("Stock and cost values cannot be negative.");
        }

        if (request.LeadTimeDays < 0)
        {
            throw new InvalidOperationException("Lead time cannot be negative.");
        }
    }

    private async Task<string> GenerateItemCodeAsync(CancellationToken ct)
    {
        var prefix = $"ITEM-{DateTime.UtcNow:yyyyMM}-";
        var nextNumber = await dbContext.InvItems
            .IgnoreQueryFilters()
            .Where(x => x.ItemCode.StartsWith(prefix))
            .CountAsync(ct) + 1;

        return $"{prefix}{nextNumber:D4}";
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

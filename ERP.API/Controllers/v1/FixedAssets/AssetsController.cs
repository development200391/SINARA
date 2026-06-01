using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/assets")]
public sealed class AssetsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaAssets
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Location)
            .Include(x => x.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.AssetCode.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(search)) ||
                (x.VendorName != null && x.VendorName.ToLower().Contains(search)) ||
                x.Category.Name.ToLower().Contains(search) ||
                x.Location.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.AssetCode))
        {
            var code = request.AssetCode.Trim().ToLowerInvariant();
            query = query.Where(x => x.AssetCode.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == request.LocationId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.BookValueFrom.HasValue)
        {
            query = query.Where(x => x.BookValue >= request.BookValueFrom.Value);
        }

        if (request.BookValueTo.HasValue)
        {
            query = query.Where(x => x.BookValue <= request.BookValueTo.Value);
        }

        if (request.AcquisitionDateFrom.HasValue)
        {
            query = query.Where(x => x.AcquisitionDate >= request.AcquisitionDateFrom.Value);
        }

        if (request.AcquisitionDateTo.HasValue)
        {
            query = query.Where(x => x.AcquisitionDate <= request.AcquisitionDateTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "assetcode" => isDesc ? query.OrderByDescending(x => x.AssetCode) : query.OrderBy(x => x.AssetCode),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "categoryname" => isDesc ? query.OrderByDescending(x => x.Category.Name).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.Category.Name).ThenBy(x => x.AssetCode),
            "locationname" => isDesc ? query.OrderByDescending(x => x.Location.Name).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.Location.Name).ThenBy(x => x.AssetCode),
            "acquisitiondate" => isDesc ? query.OrderByDescending(x => x.AcquisitionDate).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.AcquisitionDate).ThenBy(x => x.AssetCode),
            "acquisitioncost" => isDesc ? query.OrderByDescending(x => x.AcquisitionCost).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.AcquisitionCost).ThenBy(x => x.AssetCode),
            "bookvalue" => isDesc ? query.OrderByDescending(x => x.BookValue).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.BookValue).ThenBy(x => x.AssetCode),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.Status).ThenBy(x => x.AssetCode),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.AssetCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.AssetCode),
            _ => isDesc ? query.OrderByDescending(x => x.AssetCode) : query.OrderBy(x => x.AssetCode)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x, x.Documents.Count))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.FaAssets
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status != AssetStatus.Disposed)
            .OrderBy(x => x.AssetCode)
            .Select(x => new FixedAssetOptionDto
            {
                Id = x.Id,
                Label = x.AssetCode + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var asset = await dbContext.FaAssets
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Location)
            .Include(x => x.Department)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (asset is null)
        {
            return NotFound();
        }

        var schedules = await dbContext.FaDepreciationSchedules
            .AsNoTracking()
            .Where(x => x.AssetId == id)
            .OrderBy(x => x.PeriodYear)
            .ThenBy(x => x.PeriodMonth)
            .Select(x => new FixedAssetScheduleDto
            {
                Id = x.Id,
                PeriodYear = x.PeriodYear,
                PeriodMonth = x.PeriodMonth,
                DepreciationDate = x.DepreciationDate,
                DepreciationAmount = x.DepreciationAmount,
                AccumulatedDepreciation = x.AccumulatedDepreciation,
                BookValue = x.BookValue,
                Status = x.Status
            })
            .ToListAsync(ct);

        var transfers = await dbContext.FaAssetTransfers
            .AsNoTracking()
            .Include(x => x.Asset)
            .Include(x => x.FromLocation)
            .Include(x => x.ToLocation)
            .Include(x => x.FromDepartment)
            .Include(x => x.ToDepartment)
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.TransferDate)
            .Select(x => new FixedAssetTransferDto
            {
                Id = x.Id,
                TransferNo = x.TransferNo,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                TransferDate = x.TransferDate,
                FromLocationId = x.FromLocationId,
                FromLocationName = x.FromLocation.Name,
                ToLocationId = x.ToLocationId,
                ToLocationName = x.ToLocation.Name,
                FromDepartmentId = x.FromDepartmentId,
                FromDepartmentName = x.FromDepartment != null ? x.FromDepartment.Name : null,
                ToDepartmentId = x.ToDepartmentId,
                ToDepartmentName = x.ToDepartment != null ? x.ToDepartment.Name : null,
                Reason = x.Reason,
                Status = x.Status
            })
            .ToListAsync(ct);

        var maintenance = await dbContext.FaMaintenanceOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.OrderDate)
            .Select(x => new FixedAssetMaintenanceOrderDto
            {
                Id = x.Id,
                WorkOrderNo = x.WorkOrderNo,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                OrderDate = x.OrderDate,
                MaintenanceType = x.MaintenanceType,
                VendorName = x.VendorName,
                Cost = x.Cost,
                IsCapitalized = x.IsCapitalized,
                Status = x.Status,
                Notes = x.Notes
            })
            .ToListAsync(ct);

        var disposals = await dbContext.FaDisposals
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.DisposalDate)
            .Select(x => new FixedAssetDisposalDto
            {
                Id = x.Id,
                DisposalNo = x.DisposalNo,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                DisposalDate = x.DisposalDate,
                DisposalType = x.DisposalType,
                SaleAmount = x.SaleAmount,
                DisposalExpense = x.DisposalExpense,
                GainLossAmount = x.GainLossAmount,
                Status = x.Status,
                Notes = x.Notes
            })
            .ToListAsync(ct);

        var revaluations = await dbContext.FaRevaluations
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.RevaluationDate)
            .Select(x => new FixedAssetRevaluationDto
            {
                Id = x.Id,
                RevaluationNo = x.RevaluationNo,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                RevaluationDate = x.RevaluationDate,
                OldBookValue = x.OldBookValue,
                NewBookValue = x.NewBookValue,
                ImpairmentAmount = x.ImpairmentAmount,
                Status = x.Status,
                Notes = x.Notes
            })
            .ToListAsync(ct);

        var histories = await dbContext.FaAssetHistories
            .AsNoTracking()
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.EventDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new FixedAssetHistoryDto
            {
                Id = x.Id,
                EventDate = x.EventDate,
                EventType = x.EventType,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                AmountChange = x.AmountChange
            })
            .ToListAsync(ct);

        return Ok(new FixedAssetDetailDto
        {
            Asset = MapDto(asset, asset.Documents.Count),
            DepreciationSchedules = schedules,
            Transfers = transfers,
            MaintenanceOrders = maintenance,
            Disposals = disposals,
            Revaluations = revaluations,
            Histories = histories
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetDto request, CancellationToken ct)
    {
        try
        {
            await ValidateAssetDependenciesAsync(request, ct);
            ValidateAssetRequest(request);

            var category = await dbContext.FaAssetCategories
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.CategoryId, ct);

            var generatedCode = await ResolveAssetCodeAsync(request.AssetCode, category.Code, request.CategoryId, ct);

            var duplicate = await dbContext.FaAssets
                .IgnoreQueryFilters()
                .AnyAsync(x => x.AssetCode == generatedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Asset code already exists." });
            }

            var entity = new FaAsset
            {
                AssetCode = generatedCode,
                Name = NormalizeRequired(request.Name, "Asset name is required."),
                CategoryId = request.CategoryId,
                LocationId = request.LocationId,
                DepartmentId = NormalizeOptionalId(request.DepartmentId),
                AcquisitionDate = request.AcquisitionDate,
                InServiceDate = request.InServiceDate,
                AcquisitionCost = NormalizeAmount(request.AcquisitionCost),
                SalvageValue = NormalizeAmount(request.SalvageValue),
                UsefulLifeMonths = request.UsefulLifeMonths,
                DepreciationMethod = request.DepreciationMethod,
                DepreciationRate = NormalizeRate(request.DepreciationRate),
                AccumulatedDepreciation = 0m,
                BookValue = NormalizeAmount(request.AcquisitionCost),
                Status = request.Status,
                SerialNumber = NormalizeOptional(request.SerialNumber),
                VendorName = NormalizeOptional(request.VendorName),
                Description = NormalizeOptional(request.Description),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaAssets.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            await RebuildSchedulesAsync(entity, replaceExisting: true, ct);
            await AddHistoryAsync(entity.Id, entity.InServiceDate, AssetHistoryType.Registration, entity.AssetCode, "Initial asset registration", entity.AcquisitionCost, ct);

            var created = await dbContext.FaAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Location)
                .Include(x => x.Department)
                .Include(x => x.Documents)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(created, created.Documents.Count));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaAssets
                .Include(x => x.DepreciationSchedules)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            await ValidateAssetDependenciesAsync(request, ct);
            ValidateAssetRequest(request);

            var category = await dbContext.FaAssetCategories
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.CategoryId, ct);

            var resolvedCode = await ResolveAssetCodeAsync(request.AssetCode, category.Code, request.CategoryId, ct, id);

            var duplicate = await dbContext.FaAssets
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.AssetCode == resolvedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Asset code already exists." });
            }

            var hasProcessedSchedule = await dbContext.FaDepreciationSchedules
                .AsNoTracking()
                .AnyAsync(x => x.AssetId == id && x.Status == DepreciationScheduleStatus.Processed, ct);

            var financialChanged = entity.AcquisitionCost != request.AcquisitionCost
                || entity.SalvageValue != request.SalvageValue
                || entity.UsefulLifeMonths != request.UsefulLifeMonths
                || entity.DepreciationMethod != request.DepreciationMethod
                || entity.DepreciationRate != request.DepreciationRate
                || entity.InServiceDate != request.InServiceDate;

            if (hasProcessedSchedule && financialChanged)
            {
                return BadRequest(new { message = "Cannot change depreciation parameters because schedule is already processed." });
            }

            entity.AssetCode = resolvedCode;
            entity.Name = NormalizeRequired(request.Name, "Asset name is required.");
            entity.CategoryId = request.CategoryId;
            entity.LocationId = request.LocationId;
            entity.DepartmentId = NormalizeOptionalId(request.DepartmentId);
            entity.AcquisitionDate = request.AcquisitionDate;
            entity.InServiceDate = request.InServiceDate;
            entity.AcquisitionCost = NormalizeAmount(request.AcquisitionCost);
            entity.SalvageValue = NormalizeAmount(request.SalvageValue);
            entity.UsefulLifeMonths = request.UsefulLifeMonths;
            entity.DepreciationMethod = request.DepreciationMethod;
            entity.DepreciationRate = NormalizeRate(request.DepreciationRate);
            entity.Status = request.Status;
            entity.SerialNumber = NormalizeOptional(request.SerialNumber);
            entity.VendorName = NormalizeOptional(request.VendorName);
            entity.Description = NormalizeOptional(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (!hasProcessedSchedule)
            {
                entity.AccumulatedDepreciation = 0m;
                entity.BookValue = entity.AcquisitionCost;
                await dbContext.SaveChangesAsync(ct);
                await RebuildSchedulesAsync(entity, replaceExisting: true, ct);
            }
            else
            {
                await dbContext.SaveChangesAsync(ct);
            }

            var updated = await dbContext.FaAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Location)
                .Include(x => x.Department)
                .Include(x => x.Documents)
                .FirstAsync(x => x.Id == id, ct);

            return Ok(MapDto(updated, updated.Documents.Count));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaAssets
            .Include(x => x.DepreciationSchedules)
            .Include(x => x.Transfers)
            .Include(x => x.MaintenanceOrders)
            .Include(x => x.Disposals)
            .Include(x => x.Revaluations)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.DepreciationSchedules.Any(x => x.Status == DepreciationScheduleStatus.Processed)
            || entity.Transfers.Count > 0
            || entity.MaintenanceOrders.Count > 0
            || entity.Disposals.Count > 0
            || entity.Revaluations.Count > 0)
        {
            return BadRequest(new { message = "Asset cannot be deleted because transactions already exist." });
        }

        dbContext.FaAssets.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateAssetDependenciesAsync(FixedAssetDto request, CancellationToken ct)
    {
        if (request.CategoryId <= 0)
        {
            throw new InvalidOperationException("Category is required.");
        }

        var categoryExists = await dbContext.FaAssetCategories
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (request.LocationId <= 0)
        {
            throw new InvalidOperationException("Location is required.");
        }

        var locationExists = await dbContext.FaLocations
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.LocationId, ct);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location not found.");
        }

        if (request.DepartmentId is > 0)
        {
            var departmentExists = await dbContext.HrDepartments
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DepartmentId.Value, ct);
            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found.");
            }
        }
    }

    private static void ValidateAssetRequest(FixedAssetDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Asset name is required.");
        }

        if (request.AcquisitionDate > request.InServiceDate)
        {
            throw new InvalidOperationException("In-service date must be on or after acquisition date.");
        }

        if (request.AcquisitionCost < 0)
        {
            throw new InvalidOperationException("Acquisition cost cannot be negative.");
        }

        if (request.SalvageValue < 0)
        {
            throw new InvalidOperationException("Salvage value cannot be negative.");
        }

        if (request.SalvageValue > request.AcquisitionCost)
        {
            throw new InvalidOperationException("Salvage value cannot exceed acquisition cost.");
        }

        if (request.UsefulLifeMonths <= 0)
        {
            throw new InvalidOperationException("Useful life months must be greater than zero.");
        }

        if (request.DepreciationRate.HasValue && (request.DepreciationRate.Value <= 0 || request.DepreciationRate.Value > 100))
        {
            throw new InvalidOperationException("Depreciation rate must be between 0 and 100.");
        }

        if (request.DepreciationMethod == DepreciationMethod.DecliningBalance && !request.DepreciationRate.HasValue)
        {
            throw new InvalidOperationException("Depreciation rate is required for declining balance method.");
        }
    }

    private async Task<string> ResolveAssetCodeAsync(string? inputCode, string categoryCode, int categoryId, CancellationToken ct, int? excludeId = null)
    {
        if (!string.IsNullOrWhiteSpace(inputCode))
        {
            return inputCode.Trim().ToUpperInvariant();
        }

        var prefix = $"FA-{categoryCode.Trim().ToUpperInvariant()}-";

        var existingCodes = await dbContext.FaAssets
            .IgnoreQueryFilters()
            .Where(x => x.CategoryId == categoryId && x.AssetCode.StartsWith(prefix) && (!excludeId.HasValue || x.Id != excludeId.Value))
            .Select(x => x.AssetCode)
            .ToListAsync(ct);

        var maxSeq = 0;
        foreach (var code in existingCodes)
        {
            if (code.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = code[prefix.Length..];
            if (int.TryParse(suffix, out var parsed) && parsed > maxSeq)
            {
                maxSeq = parsed;
            }
        }

        return $"{prefix}{maxSeq + 1:00000}";
    }

    private async Task RebuildSchedulesAsync(FaAsset asset, bool replaceExisting, CancellationToken ct)
    {
        if (replaceExisting)
        {
            var existing = await dbContext.FaDepreciationSchedules
                .Where(x => x.AssetId == asset.Id)
                .ToListAsync(ct);

            if (existing.Count > 0)
            {
                dbContext.FaDepreciationSchedules.RemoveRange(existing);
                await dbContext.SaveChangesAsync(ct);
            }
        }

        var depreciableBase = Math.Max(asset.AcquisitionCost - asset.SalvageValue, 0m);
        if (depreciableBase <= 0 || asset.UsefulLifeMonths <= 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var runningAccumulated = 0m;
        var openingBookValue = asset.AcquisitionCost;

        var schedules = new List<FaDepreciationSchedule>();
        for (var monthIndex = 0; monthIndex < asset.UsefulLifeMonths; monthIndex++)
        {
            var depreciationDate = asset.InServiceDate.AddMonths(monthIndex);
            decimal depreciationAmount;

            if (asset.DepreciationMethod == DepreciationMethod.StraightLine)
            {
                depreciationAmount = decimal.Round(depreciableBase / asset.UsefulLifeMonths, 2, MidpointRounding.AwayFromZero);

                if (monthIndex == asset.UsefulLifeMonths - 1)
                {
                    depreciationAmount = depreciableBase - runningAccumulated;
                }
            }
            else
            {
                var rate = asset.DepreciationRate ?? 20m;
                depreciationAmount = decimal.Round((openingBookValue * rate / 100m) / 12m, 2, MidpointRounding.AwayFromZero);
                var maxAllowed = Math.Max(openingBookValue - asset.SalvageValue, 0m);

                if (depreciationAmount > maxAllowed)
                {
                    depreciationAmount = maxAllowed;
                }

                if (monthIndex == asset.UsefulLifeMonths - 1 && depreciationAmount < maxAllowed)
                {
                    depreciationAmount = maxAllowed;
                }
            }

            if (depreciationAmount < 0)
            {
                depreciationAmount = 0m;
            }

            runningAccumulated += depreciationAmount;
            var bookValue = asset.AcquisitionCost - runningAccumulated;
            if (bookValue < asset.SalvageValue)
            {
                bookValue = asset.SalvageValue;
            }

            openingBookValue = bookValue;

            schedules.Add(new FaDepreciationSchedule
            {
                AssetId = asset.Id,
                PeriodYear = (short)depreciationDate.Year,
                PeriodMonth = (byte)depreciationDate.Month,
                DepreciationDate = depreciationDate,
                DepreciationAmount = decimal.Round(depreciationAmount, 2, MidpointRounding.AwayFromZero),
                AccumulatedDepreciation = decimal.Round(runningAccumulated, 2, MidpointRounding.AwayFromZero),
                BookValue = decimal.Round(bookValue, 2, MidpointRounding.AwayFromZero),
                Status = DepreciationScheduleStatus.Pending,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                CreatedAt = now
            });
        }

        if (schedules.Count > 0)
        {
            dbContext.FaDepreciationSchedules.AddRange(schedules);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task AddHistoryAsync(
        int assetId,
        DateOnly eventDate,
        AssetHistoryType eventType,
        string? referenceNo,
        string? description,
        decimal? amountChange,
        CancellationToken ct)
    {
        var normalizedReference = NormalizeOptional(referenceNo)?.ToUpperInvariant();

        var exists = await dbContext.FaAssetHistories
            .AnyAsync(x =>
                x.AssetId == assetId
                && x.EventDate == eventDate
                && x.EventType == eventType
                && x.ReferenceNo == normalizedReference,
                ct);

        if (exists)
        {
            return;
        }

        dbContext.FaAssetHistories.Add(new FaAssetHistory
        {
            AssetId = assetId,
            EventDate = eventDate,
            EventType = eventType,
            ReferenceNo = normalizedReference,
            Description = NormalizeOptional(description),
            AmountChange = amountChange,
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
        });

        await dbContext.SaveChangesAsync(ct);
    }

    private static int? NormalizeOptionalId(int? value) => value is > 0 ? value : null;

    private static decimal NormalizeAmount(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal? NormalizeRate(decimal? value)
        => value.HasValue ? decimal.Round(value.Value, 4, MidpointRounding.AwayFromZero) : null;

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FixedAssetDto MapDto(FaAsset entity, int documentCount)
    {
        return new FixedAssetDto
        {
            Id = entity.Id,
            AssetCode = entity.AssetCode,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            CategoryCode = entity.Category?.Code ?? string.Empty,
            CategoryName = entity.Category?.Name ?? string.Empty,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code ?? string.Empty,
            LocationName = entity.Location?.Name ?? string.Empty,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.Name,
            AcquisitionDate = entity.AcquisitionDate,
            InServiceDate = entity.InServiceDate,
            AcquisitionCost = entity.AcquisitionCost,
            SalvageValue = entity.SalvageValue,
            UsefulLifeMonths = entity.UsefulLifeMonths,
            DepreciationMethod = entity.DepreciationMethod,
            DepreciationRate = entity.DepreciationRate,
            AccumulatedDepreciation = entity.AccumulatedDepreciation,
            BookValue = entity.BookValue,
            Status = entity.Status,
            SerialNumber = entity.SerialNumber,
            VendorName = entity.VendorName,
            Description = entity.Description,
            IsActive = entity.IsActive,
            DocumentCount = documentCount
        };
    }
}

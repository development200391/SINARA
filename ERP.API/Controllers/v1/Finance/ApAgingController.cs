using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/ap/aging")]
public sealed class ApAgingController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApAgingPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var asOfDate = request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var query = dbContext.FinApInvoices
            .AsNoTracking()
            .Where(x => x.OutstandingAmount > 0 && x.Status != FinanceApInvoiceStatus.Cancelled)
            .AsQueryable();

        if (request.VendorId.HasValue)
        {
            query = query.Where(x => x.VendorId == request.VendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Vendor.Code.ToLower().Contains(search) ||
                x.Vendor.Name.ToLower().Contains(search) ||
                x.InvoiceNo.ToLower().Contains(search));
        }

        var raw = await query
            .Select(x => new
            {
                x.VendorId,
                VendorCode = x.Vendor.Code,
                VendorName = x.Vendor.Name,
                x.InvoiceDate,
                x.DueDate,
                x.OutstandingAmount
            })
            .ToListAsync(ct);

        var grouped = raw
            .GroupBy(x => new { x.VendorId, x.VendorCode, x.VendorName })
            .Select(group =>
            {
                decimal current = 0m;
                decimal bucket1To30 = 0m;
                decimal bucket31To60 = 0m;
                decimal bucket61To90 = 0m;
                decimal bucketOver90 = 0m;

                foreach (var row in group)
                {
                    var daysPastDue = asOfDate.DayNumber - row.DueDate.DayNumber;
                    if (daysPastDue <= 0)
                    {
                        current += row.OutstandingAmount;
                    }
                    else if (daysPastDue <= 30)
                    {
                        bucket1To30 += row.OutstandingAmount;
                    }
                    else if (daysPastDue <= 60)
                    {
                        bucket31To60 += row.OutstandingAmount;
                    }
                    else if (daysPastDue <= 90)
                    {
                        bucket61To90 += row.OutstandingAmount;
                    }
                    else
                    {
                        bucketOver90 += row.OutstandingAmount;
                    }
                }

                return new ApAgingRowDto
                {
                    VendorId = group.Key.VendorId,
                    VendorCode = group.Key.VendorCode,
                    VendorName = group.Key.VendorName,
                    CurrentAmount = current,
                    Bucket1To30 = bucket1To30,
                    Bucket31To60 = bucket31To60,
                    Bucket61To90 = bucket61To90,
                    BucketOver90 = bucketOver90,
                    TotalOutstanding = current + bucket1To30 + bucket31To60 + bucket61To90 + bucketOver90,
                    OldestInvoiceDate = group.Min(x => (DateOnly?)x.InvoiceDate),
                    LatestDueDate = group.Max(x => (DateOnly?)x.DueDate)
                };
            })
            .ToList();

        if (request.OutstandingMin.HasValue)
        {
            grouped = grouped.Where(x => x.TotalOutstanding >= request.OutstandingMin.Value).ToList();
        }

        if (request.OutstandingMax.HasValue)
        {
            grouped = grouped.Where(x => x.TotalOutstanding <= request.OutstandingMax.Value).ToList();
        }

        var sorted = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "vendorcode" => isDesc ? grouped.OrderByDescending(x => x.VendorCode) : grouped.OrderBy(x => x.VendorCode),
            "vendorname" => isDesc ? grouped.OrderByDescending(x => x.VendorName) : grouped.OrderBy(x => x.VendorName),
            "currentamount" => isDesc ? grouped.OrderByDescending(x => x.CurrentAmount) : grouped.OrderBy(x => x.CurrentAmount),
            "bucket1to30" => isDesc ? grouped.OrderByDescending(x => x.Bucket1To30) : grouped.OrderBy(x => x.Bucket1To30),
            "bucket31to60" => isDesc ? grouped.OrderByDescending(x => x.Bucket31To60) : grouped.OrderBy(x => x.Bucket31To60),
            "bucket61to90" => isDesc ? grouped.OrderByDescending(x => x.Bucket61To90) : grouped.OrderBy(x => x.Bucket61To90),
            "bucketover90" => isDesc ? grouped.OrderByDescending(x => x.BucketOver90) : grouped.OrderBy(x => x.BucketOver90),
            "totaloutstanding" => isDesc ? grouped.OrderByDescending(x => x.TotalOutstanding) : grouped.OrderBy(x => x.TotalOutstanding),
            _ => isDesc ? grouped.OrderByDescending(x => x.TotalOutstanding).ThenByDescending(x => x.VendorCode) : grouped.OrderByDescending(x => x.TotalOutstanding).ThenBy(x => x.VendorCode)
        };

        var totalCount = grouped.Count;
        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<ApAgingRowDto>.Create(items, totalCount, page, pageSize));
    }
}

using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaDepreciationRun : BaseEntity
{
    public string RunNo { get; set; } = string.Empty;
    public short PeriodYear { get; set; }
    public byte PeriodMonth { get; set; }
    public DateOnly RunDate { get; set; }
    public int TotalAssetCount { get; set; }
    public decimal TotalDepreciationAmount { get; set; }
    public DepreciationRunStatus Status { get; set; } = DepreciationRunStatus.Draft;
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public SysUser? ApprovedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<FaDepreciationSchedule> Schedules { get; set; } = new List<FaDepreciationSchedule>();
}

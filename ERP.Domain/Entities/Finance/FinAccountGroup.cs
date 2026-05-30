using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinAccountGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public FinanceAccountType Type { get; set; }
    public FinanceNormalBalance NormalBalance { get; set; } = FinanceNormalBalance.Debit;
    public int? ParentGroupId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public FinAccountGroup? ParentGroup { get; set; }
    public ICollection<FinAccountGroup> ChildGroups { get; set; } = new List<FinAccountGroup>();
    public ICollection<FinAccount> Accounts { get; set; } = new List<FinAccount>();
}

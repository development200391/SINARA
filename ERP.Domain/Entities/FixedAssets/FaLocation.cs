using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaLocation : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;

    public HrDepartment? Department { get; set; }
    public HrEmployee? Manager { get; set; }
    public ICollection<FaAsset> Assets { get; set; } = new List<FaAsset>();
}

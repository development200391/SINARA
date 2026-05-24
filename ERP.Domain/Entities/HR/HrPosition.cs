namespace ERP.Domain.Entities.HR;

public sealed class HrPosition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;

    public HrDepartment Department { get; set; } = null!;
    public ICollection<HrEmployee> Employees { get; set; } = new List<HrEmployee>();
}

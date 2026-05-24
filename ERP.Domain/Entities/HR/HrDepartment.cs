namespace ERP.Domain.Entities.HR;

public sealed class HrDepartment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    public HrEmployee? Manager { get; set; }
    public HrDepartment? ParentDepartment { get; set; }
    public ICollection<HrDepartment> ChildDepartments { get; set; } = new List<HrDepartment>();
    public ICollection<HrPosition> Positions { get; set; } = new List<HrPosition>();
    public ICollection<HrEmployee> Employees { get; set; } = new List<HrEmployee>();
}

using ERP.Application.DTOs.Common;

namespace ERP.Application.DTOs.HR;

public sealed class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class DepartmentPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? ManagerId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}

using ERP.Application.DTOs.Common;

namespace ERP.Application.DTOs.HR;

public sealed class PositionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsActive { get; set; }
}

public sealed class PositionPagedRequest : PagedRequest
{
    public int? DepartmentId { get; set; }
}

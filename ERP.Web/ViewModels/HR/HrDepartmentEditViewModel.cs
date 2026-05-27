using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrDepartmentEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public int? ManagerId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<LookupDto> Managers { get; set; } = [];
    public IReadOnlyList<DepartmentDto> ParentDepartments { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPositionEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int Level { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
}

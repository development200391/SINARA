using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveTypeEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Range(1, 365)]
    public int MaxDaysPerYear { get; set; } = 12;

    public bool IsCarryOver { get; set; }
    public bool IsActive { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.HR;

public sealed class HrHolidayEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Holiday Date")]
    [DataType(DataType.Date)]
    public DateOnly HolidayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Holiday Type")]
    public HolidayType HolidayType { get; set; } = HolidayType.National;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Applies To")]
    [MaxLength(100)]
    public string AppliesTo { get; set; } = "all";
}

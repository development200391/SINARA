using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Document;
using ERP.Application.DTOs.HR;
using Microsoft.AspNetCore.Http;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveRequestEditViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(500)]
    public string? Reason { get; set; }

    public IFormFile? AttachmentFile { get; set; }
    public int? AttachmentCategoryId { get; set; }

    [MaxLength(500)]
    public string? AttachmentDescription { get; set; }

    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public IReadOnlyList<LookupDto> LeaveTypes { get; set; } = [];
    public IReadOnlyList<DocumentCategoryDto> AttachmentCategories { get; set; } = [];
    public IReadOnlyList<DocumentDto> Documents { get; set; } = [];
}

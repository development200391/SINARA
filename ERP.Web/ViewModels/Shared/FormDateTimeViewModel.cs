namespace ERP.Web.ViewModels.Shared;

public sealed class FormDateTimeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Must already be in the timezone that should be displayed to the user (this app has no
    /// per-user timezone setting, so callers convert from UTC using the server's local timezone,
    /// see <see cref="ERP.Web.Controllers.HR.HrAttendanceController"/>). On submit the bound
    /// value comes back as DateTimeKind.Unspecified; the caller is responsible for re-attaching
    /// that same timezone before persisting/converting to UTC.
    /// </summary>
    public DateTime? Value { get; set; }
    public bool IsRequired { get; set; }
    public string? Error { get; set; }
    public IDictionary<string, string>? HtmlAttributes { get; set; }
}

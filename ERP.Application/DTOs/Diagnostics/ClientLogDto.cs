namespace ERP.Application.DTOs.Diagnostics;

public sealed class ClientLogRequest
{
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ReferenceCorrelationId { get; set; }
    public string? Platform { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}

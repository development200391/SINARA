namespace ERP.Application.Services.Approval;

/// <summary>
/// Implemented per source module (keyed by <see cref="ReferenceType"/>) and resolved by the
/// routing engine via a registry lookup over all registered implementations — not a generic event bus.
/// </summary>
public interface IApprovalCallbackService
{
    string ReferenceType { get; }

    Task OnApprovedAsync(int referenceId, int actorUserId, CancellationToken ct = default);

    Task OnRejectedAsync(int referenceId, int actorUserId, string? comment, CancellationToken ct = default);

    Task OnCancelledAsync(int referenceId, int actorUserId, CancellationToken ct = default);
}

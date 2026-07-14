namespace ERP.Web.Services;

/// <summary>
/// Maps an APV ApprovalRequest's (ReferenceType, ReferenceId) to the source module's own details page,
/// so the generic Approval Inbox/My Requests grids can link straight to the full record (reason,
/// attachments, etc.) instead of only showing the generic Template/Amount/Requester columns.
/// Add one entry per source module as it gets wired into General Approval — see
/// ReadMeGeneralApproval.md "Integrasi ke modul lain".
/// </summary>
public static class ApprovalReferenceLinkResolver
{
    private static readonly Dictionary<string, Func<int, string>> Templates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hr_leave_requests"] = referenceId => $"/hr/leave/requests/details/{referenceId}"
    };

    public static string? Resolve(string referenceType, int referenceId) =>
        Templates.TryGetValue(referenceType, out var buildUrl) ? buildUrl(referenceId) : null;
}

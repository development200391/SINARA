namespace ERP.Web.Services;

public sealed class MenuPermissionResult
{
    public static readonly MenuPermissionResult NoMatch = new();

    public bool IsMenuMatched { get; init; }
    public int? MenuId { get; init; }
    public string MenuUrl { get; init; } = string.Empty;
    public MenuPermissionFlags Permission { get; init; } = MenuPermissionFlags.None;
}

namespace ERP.Web.Services;

public enum MenuPermissionAction
{
    View,
    Create,
    Edit,
    Delete
}

public sealed class MenuPermissionFlags
{
    public static readonly MenuPermissionFlags None = new();

    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }

    public bool Allows(MenuPermissionAction action) => action switch
    {
        MenuPermissionAction.View => CanView,
        MenuPermissionAction.Create => CanCreate,
        MenuPermissionAction.Edit => CanEdit,
        MenuPermissionAction.Delete => CanDelete,
        _ => false
    };
}

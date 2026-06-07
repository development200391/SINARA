namespace ERP.Web.ViewModels.Shared;

public sealed class AuthFormCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string BodyPartialViewName { get; init; } = string.Empty;
    public object? BodyModel { get; init; }
}


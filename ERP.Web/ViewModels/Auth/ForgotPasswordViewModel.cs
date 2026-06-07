using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Auth;

public sealed class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}


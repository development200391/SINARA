namespace ERP.Application.Services.Config;

public interface IUserCredentialEmailService
{
    Task<bool> IsEmailActiveAsync(string email, CancellationToken ct = default);
    Task SendCredentialAsync(string email, string fullName, string username, string temporaryPassword, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string fullName, string resetToken, DateTimeOffset expiresAt, CancellationToken ct = default);
}


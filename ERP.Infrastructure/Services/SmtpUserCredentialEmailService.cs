using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using ERP.Application.Options;
using ERP.Application.Services.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Infrastructure.Services;

public sealed class SmtpUserCredentialEmailService(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<SmtpUserCredentialEmailService> logger) : IUserCredentialEmailService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task<bool> IsEmailActiveAsync(string email, CancellationToken ct = default)
    {
        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            return false;
        }

        var atIndex = normalizedEmail.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == normalizedEmail.Length - 1)
        {
            return false;
        }

        var domain = normalizedEmail[(atIndex + 1)..];
        try
        {
            await Dns.GetHostEntryAsync(domain);
            return true;
        }
        catch (Exception ex) when (ex is SocketException or ArgumentException)
        {
            logger.LogWarning(ex, "Failed to resolve email domain {Domain} for {Email}", domain, normalizedEmail);
            return false;
        }
    }

    public async Task SendCredentialAsync(string email, string fullName, string username, string temporaryPassword, CancellationToken ct = default)
    {
        if (!_smtp.Enabled)
        {
            logger.LogInformation("SMTP email service is disabled. Skip sending credentials to {Email}.", email);
            return;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            throw new InvalidOperationException("Email format is invalid.");
        }

        ValidateSmtpConfiguration();

        var body = BuildCredentialEmailBody(fullName, username, temporaryPassword);
        await SendEmailAsync(normalizedEmail, "SINARA ERP - Account Credentials", body, ct);
    }

    public async Task SendPasswordResetAsync(string email, string fullName, string resetToken, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        if (!_smtp.Enabled)
        {
            logger.LogInformation("SMTP email service is disabled. Skip sending reset password email to {Email}.", email);
            return;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            throw new InvalidOperationException("Email format is invalid.");
        }

        ValidateSmtpConfiguration();

        var resetUrl = BuildPasswordResetUrl(normalizedEmail, resetToken);
        var body = BuildPasswordResetEmailBody(fullName, resetUrl, expiresAt);

        await SendEmailAsync(normalizedEmail, "SINARA ERP - Reset Password", body, ct);
    }

    private async Task SendEmailAsync(string recipientEmail, string subject, string body, CancellationToken ct)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.FromEmail, _smtp.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(recipientEmail);

        using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = _smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = Math.Max(5, _smtp.TimeoutSeconds) * 1000
        };

        if (!string.IsNullOrWhiteSpace(_smtp.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_smtp.Username, _smtp.Password);
        }

        await smtpClient.SendMailAsync(message, ct);
    }

    private void ValidateSmtpConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host))
        {
            throw new InvalidOperationException("SMTP host is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_smtp.FromEmail))
        {
            throw new InvalidOperationException("SMTP sender email is not configured.");
        }
    }

    private string BuildPasswordResetUrl(string email, string resetToken)
    {
        var template = _smtp.PasswordResetUrlTemplate?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new InvalidOperationException("SMTP password reset URL template is not configured.");
        }

        if (!template.Contains("{email}", StringComparison.OrdinalIgnoreCase)
            || !template.Contains("{token}", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SMTP password reset URL template must contain {email} and {token} placeholders.");
        }

        return template
            .Replace("{email}", Uri.EscapeDataString(email), StringComparison.OrdinalIgnoreCase)
            .Replace("{token}", Uri.EscapeDataString(resetToken), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryNormalizeEmail(string email, out string normalizedEmail)
    {
        normalizedEmail = string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email.Trim());
            normalizedEmail = address.Address;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string BuildCredentialEmailBody(string fullName, string username, string temporaryPassword)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? username : fullName);
        var safeUsername = WebUtility.HtmlEncode(username);
        var safePassword = WebUtility.HtmlEncode(temporaryPassword);

        return $"""
            <p>Dear {safeName},</p>
            <p>Your SINARA ERP account has been created.</p>
            <p><strong>Username:</strong> {safeUsername}<br />
            <strong>Temporary Password:</strong> {safePassword}</p>
            <p>Please sign in and change your password immediately.</p>
            """;
    }

    private static string BuildPasswordResetEmailBody(string fullName, string resetUrl, DateTimeOffset expiresAt)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "User" : fullName);
        var safeResetUrl = WebUtility.HtmlEncode(resetUrl);
        var expiresText = WebUtility.HtmlEncode(expiresAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm"));

        return $"""
            <p>Dear {safeName},</p>
            <p>We received a request to reset your SINARA ERP password.</p>
            <p><a href="{safeResetUrl}">Click here to reset your password</a></p>
            <p>This link will expire at {expiresText}.</p>
            <p>If you did not request this change, you can ignore this email.</p>
            """;
    }
}


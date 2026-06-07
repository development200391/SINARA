namespace ERP.Application.Options;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "SINARA ERP";
    public int TimeoutSeconds { get; set; } = 15;
    public string PasswordResetUrlTemplate { get; set; } = "https://localhost:60100/auth/reset-password?email={email}&token={token}";
}


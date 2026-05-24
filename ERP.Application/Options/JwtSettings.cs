namespace ERP.Application.Options;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = "ERPSystem";
    public string Audience { get; set; } = "ERPClients";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public string SigningKey { get; set; } = "ChangeMe_SigningKey_AtLeast_32_Chars_Long_12345";
}

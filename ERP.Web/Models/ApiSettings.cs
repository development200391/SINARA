namespace ERP.Web.Models;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";
    public string BaseUrl { get; set; } = "http://localhost:8081";
}

namespace ERP.Web.Services.Localization;

public interface ITextLocalizer
{
    string CurrentLanguage { get; }
    string this[string key] { get; }
}

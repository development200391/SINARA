using System.Globalization;
using ERP.Web.Services.Localization;
using ERP.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.ViewComponents;

public sealed class TextNumberViewComponent(ITextLocalizer textLocalizer) : ViewComponent
{
    public IViewComponentResult Invoke(
        string? text = null,
        decimal? value = null,
        int decimals = 2,
        bool useGrouping = true,
        string? cssClass = null,
        string nullText = "-")
    {
        var safeDecimals = Math.Clamp(decimals, 0, 6);
        var culture = ResolveCulture(textLocalizer.CurrentLanguage);
        var format = useGrouping ? $"N{safeDecimals}" : $"F{safeDecimals}";

        var numericValue = value;
        if (!numericValue.HasValue && !string.IsNullOrWhiteSpace(text))
        {
            numericValue = TryParseNumber(text!, culture);
        }

        var hasValue = numericValue.HasValue || !string.IsNullOrWhiteSpace(text);
        var renderedText = numericValue.HasValue
            ? numericValue.Value.ToString(format, culture)
            : (!string.IsNullOrWhiteSpace(text) ? text!.Trim() : nullText);

        var model = new TextNumberViewModel
        {
            HasValue = hasValue,
            Text = renderedText,
            CssClass = cssClass ?? string.Empty
        };

        return View(model);
    }

    private static CultureInfo ResolveCulture(string? language)
    {
        return string.Equals(language, "id", StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.GetCultureInfo("id-ID")
            : CultureInfo.GetCultureInfo("en-US");
    }

    private static decimal? TryParseNumber(string rawText, CultureInfo preferredCulture)
    {
        var text = rawText.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        const NumberStyles style = NumberStyles.Number | NumberStyles.AllowLeadingSign;

        if (decimal.TryParse(text, style, preferredCulture, out var parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(text, style, CultureInfo.InvariantCulture, out parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(text, style, CultureInfo.GetCultureInfo("en-US"), out parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(text, style, CultureInfo.GetCultureInfo("id-ID"), out parsed))
        {
            return parsed;
        }

        var compact = text.Replace(" ", string.Empty);
        var lastDot = compact.LastIndexOf('.');
        var lastComma = compact.LastIndexOf(',');
        string normalized;

        if (lastDot >= 0 && lastComma >= 0)
        {
            normalized = lastDot > lastComma
                ? compact.Replace(",", string.Empty)
                : compact.Replace(".", string.Empty).Replace(',', '.');
        }
        else if (lastComma >= 0)
        {
            var commaCount = CountOccurrences(compact, ',');
            if (commaCount > 1)
            {
                normalized = compact.Replace(",", string.Empty);
            }
            else
            {
                var digitsAfter = compact.Length - lastComma - 1;
                normalized = digitsAfter == 3
                    ? compact.Replace(",", string.Empty)
                    : compact.Replace(',', '.');
            }
        }
        else
        {
            normalized = compact.Replace(",", string.Empty);
        }

        return decimal.TryParse(normalized, style, CultureInfo.InvariantCulture, out parsed)
            ? parsed
            : null;
    }

    private static int CountOccurrences(string value, char needle)
    {
        var count = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == needle)
            {
                count++;
            }
        }

        return count;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ERP.Web.Validation;

/// <summary>
/// Culture-safe replacement for <c>[Range(typeof(decimal), "min", "max")]</c>.
/// The base RangeAttribute parses its string bounds using
/// <see cref="CultureInfo.CurrentCulture"/>, which throws a FormatException
/// whenever the bound contains a "." decimal separator (e.g. "0.000001")
/// and the request thread's culture uses "," instead (e.g. id-ID).
/// </summary>
public sealed class DecimalRangeAttribute : RangeAttribute
{
    public DecimalRangeAttribute(string minimum, string maximum)
        : base(typeof(decimal), minimum, maximum)
    {
    }

    public override bool IsValid(object? value)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            return base.IsValid(value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}

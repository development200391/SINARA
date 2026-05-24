using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[AllowAnonymous]
[Route("language")]
public sealed class LanguageController : Controller
{
    [HttpGet("set")]
    public IActionResult Set(string culture = "en", string? returnUrl = null)
    {
        var language = culture is "id" ? "id" : "en";

        Response.Cookies.Append("sinara_lang", language, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}

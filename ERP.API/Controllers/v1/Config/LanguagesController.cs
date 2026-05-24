using ERP.Application.DTOs.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/languages")]
public sealed class LanguagesController : ConfigControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var languages = new List<LanguageDto>
        {
            new()
            {
                Code = "en",
                Name = "English",
                NativeName = "English",
                IsDefault = true
            },
            new()
            {
                Code = "id",
                Name = "Indonesian",
                NativeName = "Bahasa Indonesia",
                IsDefault = false
            }
        };

        return Ok(languages);
    }
}

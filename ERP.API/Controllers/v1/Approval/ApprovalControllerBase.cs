using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[ApiController]
[Authorize]
public abstract class ApprovalControllerBase : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var parsed) ? parsed : null;
    }
}

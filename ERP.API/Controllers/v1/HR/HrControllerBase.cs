using System.Security.Claims;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[ApiController]
[Authorize]
public abstract class HrControllerBase : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var parsed) ? parsed : null;
    }

    protected async Task<int?> ResolveEmployeeIdAsync(IEmployeeService employeeService, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return null;
        }

        var employee = await employeeService.GetByUserIdAsync(userId.Value, ct);
        return employee?.Id;
    }
}

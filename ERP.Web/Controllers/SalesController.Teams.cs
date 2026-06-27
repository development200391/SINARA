namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("teams")]
    public async Task<IActionResult> Teams(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? teamLeaderId = null,
        int? memberEmployeeId = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "teamleadername", "membercount", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var employeeOptionsTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var itemsTask = salesApiClient.GetTeamsAsync(accessToken, new SalesTeamPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            TeamLeaderId = teamLeaderId,
            MemberEmployeeId = memberEmployeeId,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(employeeOptionsTask, itemsTask);

        ViewData["Title"] = "Sales Teams";
        ViewData["Breadcrumb"] = "Sales / Teams";

        return View("Teams/Index", new SalesTeamsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TeamLeaderIdFilter = teamLeaderId,
            MemberEmployeeIdFilter = memberEmployeeId,
            IsActiveFilter = isActive,
            EmployeeOptions = (await employeeOptionsTask).OrderBy(x => x.Name).ToList(),
            Items = await itemsTask ?? PagedResult<SalesTeamDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("teams/create")]
    public async Task<IActionResult> CreateTeam(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var employeeOptions = (await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct)).OrderBy(x => x.Name).ToList();
        var model = new SalesTeamEditViewModel
        {
            IsActive = true,
            TeamLeaderId = employeeOptions.FirstOrDefault()?.Id ?? 0,
            EmployeeOptions = employeeOptions
        };

        ViewData["Title"] = "Create Sales Team";
        ViewData["Breadcrumb"] = "Sales / Teams / Create";

        return View("Teams/Create", model);
    }

    [HttpPost("teams/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeam(SalesTeamEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeTeamForm(model);
        model.EmployeeOptions = (await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct)).OrderBy(x => x.Name).ToList();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Sales Team";
            ViewData["Breadcrumb"] = "Sales / Teams / Create";
            return View("Teams/Create", model);
        }

        var created = await salesApiClient.CreateTeamAsync(accessToken, MapTeamRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create sales team." : created.ErrorMessage);
            ViewData["Title"] = "Create Sales Team";
            ViewData["Breadcrumb"] = "Sales / Teams / Create";
            return View("Teams/Create", model);
        }

        TempData["SuccessMessage"] = "Sales team created.";
        return RedirectToAction(nameof(Teams));
    }

    [HttpGet("teams/edit/{id:int}")]
    public async Task<IActionResult> EditTeam(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var team = await salesApiClient.GetTeamByIdAsync(accessToken, id, ct);
        if (team is null)
        {
            return NotFound();
        }

        var model = new SalesTeamEditViewModel
        {
            Id = team.Id,
            Code = team.Code,
            Name = team.Name,
            TeamLeaderId = team.TeamLeaderId,
            IsActive = team.IsActive,
            MemberEmployeeIds = team.MemberEmployeeIds.ToList(),
            EmployeeOptions = (await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct)).OrderBy(x => x.Name).ToList()
        };

        ViewData["Title"] = "Edit Sales Team";
        ViewData["Breadcrumb"] = "Sales / Teams / Edit";

        return View("Teams/Edit", model);
    }

    [HttpPost("teams/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTeam(int id, SalesTeamEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeTeamForm(model);
        model.EmployeeOptions = (await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct)).OrderBy(x => x.Name).ToList();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Sales Team";
            ViewData["Breadcrumb"] = "Sales / Teams / Edit";
            return View("Teams/Edit", model);
        }

        var updated = await salesApiClient.UpdateTeamAsync(accessToken, id, MapTeamRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update sales team." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Sales Team";
            ViewData["Breadcrumb"] = "Sales / Teams / Edit";
            return View("Teams/Edit", model);
        }

        TempData["SuccessMessage"] = "Sales team updated.";
        return RedirectToAction(nameof(Teams));
    }

    [HttpPost("teams/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeam(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await salesApiClient.DeleteTeamAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Sales team deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete sales team." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Teams));
    }
}

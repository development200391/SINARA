using System.Security.Claims;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/menus")]
public sealed class ConfigMenusController(IConfigApiClient configApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int? moduleId = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var modules = await configApiClient.GetModulesAsync(accessToken, ct);
        var selectedModuleId = moduleId ?? modules.FirstOrDefault()?.Id ?? 0;
        var menus = selectedModuleId > 0
            ? await configApiClient.GetMenusByModuleAsync(accessToken, selectedModuleId, ct)
            : [];

        ViewData["Title"] = "Menu Config";
        ViewData["Breadcrumb"] = "Configuration / Menu Config";

        return View(new ConfigMenusIndexViewModel
        {
            Modules = modules,
            SelectedModuleId = selectedModuleId,
            SelectedModule = modules.FirstOrDefault(x => x.Id == selectedModuleId),
            Menus = menus,
            MenuRows = BuildRows(menus)
        });
    }

    [HttpPost("reorder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(int moduleId, List<int> orderedMenuIds, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (moduleId <= 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var ordered = orderedMenuIds.Where(x => x > 0).Distinct().ToList();
        if (ordered.Count > 0)
        {
            var reordered = await configApiClient.ReorderMenusAsync(accessToken, moduleId, ordered, ct);
            TempData[reordered.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = reordered.IsSuccess
                ? "Menu order updated."
                : (string.IsNullOrWhiteSpace(reordered.ErrorMessage) ? "Failed to update menu order." : reordered.ErrorMessage);
        }

        return RedirectToAction(nameof(Index), new { moduleId });
    }

    [HttpPost("module/{id:int}/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModule(int id, string name, string? icon, int sortOrder, bool isActive, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var updated = await configApiClient.UpdateModuleAsync(accessToken, id, new ModuleDto
        {
            Id = id,
            Name = name,
            Icon = icon,
            SortOrder = sortOrder,
            IsActive = isActive
        }, ct);

        TempData[updated.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = updated.IsSuccess ? "Module settings updated." : (string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update module settings." : updated.ErrorMessage);

        return RedirectToAction(nameof(Index), new { moduleId = id });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int? moduleId = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var modules = await configApiClient.GetModulesAsync(accessToken, ct);
        var selectedModuleId = moduleId ?? modules.FirstOrDefault()?.Id ?? 0;
        var menus = selectedModuleId > 0
            ? await configApiClient.GetMenusByModuleAsync(accessToken, selectedModuleId, ct)
            : [];

        ViewData["Title"] = "Add Menu";
        ViewData["Breadcrumb"] = "Configuration / Menu Config / Add";

        return View(new ConfigMenuEditViewModel
        {
            ModuleId = selectedModuleId,
            SortOrder = menus.Count + 1,
            IsActive = true,
            AvailableModules = modules,
            AvailableParents = menus
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConfigMenuEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        await PopulateFormOptionsAsync(accessToken, model, null, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Add Menu";
            ViewData["Breadcrumb"] = "Configuration / Menu Config / Add";
            return View(model);
        }

        var created = await configApiClient.CreateMenuAsync(accessToken, MapToDto(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create menu." : created.ErrorMessage);
            ViewData["Title"] = "Add Menu";
            ViewData["Breadcrumb"] = "Configuration / Menu Config / Add";
            return View(model);
        }

        TempData["SuccessMessage"] = "Menu created.";
        return RedirectToAction(nameof(Index), new { moduleId = model.ModuleId });
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, int? moduleId = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var menu = await configApiClient.GetMenuByIdAsync(accessToken, id, ct);
        if (menu is null)
        {
            return NotFound();
        }

        var model = new ConfigMenuEditViewModel
        {
            Id = menu.Id,
            ModuleId = moduleId ?? menu.ModuleId,
            ParentId = menu.ParentId,
            Name = menu.Name,
            Url = menu.Url,
            Icon = menu.Icon,
            SortOrder = menu.SortOrder,
            IsActive = menu.IsActive
        };

        await PopulateFormOptionsAsync(accessToken, model, id, ct);

        ViewData["Title"] = "Edit Menu";
        ViewData["Breadcrumb"] = "Configuration / Menu Config / Edit";
        return View(model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConfigMenuEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Id = id;
        await PopulateFormOptionsAsync(accessToken, model, id, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Menu";
            ViewData["Breadcrumb"] = "Configuration / Menu Config / Edit";
            return View(model);
        }

        var updated = await configApiClient.UpdateMenuAsync(accessToken, id, MapToDto(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update menu." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Menu";
            ViewData["Breadcrumb"] = "Configuration / Menu Config / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Menu updated.";
        return RedirectToAction(nameof(Index), new { moduleId = model.ModuleId });
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int moduleId, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await configApiClient.DeleteMenuAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Menu deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Menu gagal dihapus. Pastikan tidak punya child menu." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index), new { moduleId });
    }

    private static MenuDto MapToDto(ConfigMenuEditViewModel model)
    {
        return new MenuDto
        {
            Id = model.Id,
            ModuleId = model.ModuleId,
            ParentId = model.ParentId,
            Name = model.Name,
            Url = model.Url,
            Icon = model.Icon,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        };
    }

    private async Task PopulateFormOptionsAsync(string accessToken, ConfigMenuEditViewModel model, int? editingMenuId, CancellationToken ct)
    {
        var modules = await configApiClient.GetModulesAsync(accessToken, ct);

        if (model.ModuleId <= 0)
        {
            model.ModuleId = modules.FirstOrDefault()?.Id ?? 0;
        }

        var menus = model.ModuleId > 0
            ? await configApiClient.GetMenusByModuleAsync(accessToken, model.ModuleId, ct)
            : [];

        HashSet<int> excluded = editingMenuId.HasValue
            ? GetDescendantIds(editingMenuId.Value, menus).Append(editingMenuId.Value).ToHashSet()
            : [];

        model.AvailableModules = modules;
        model.AvailableParents = menus
            .Where(x => !excluded.Contains(x.Id))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();
    }

    private static HashSet<int> GetDescendantIds(int id, IReadOnlyList<MenuDto> menus)
    {
        var descendants = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = menus.Where(x => x.ParentId == current).Select(x => x.Id).ToList();
            foreach (var child in children)
            {
                if (!descendants.Add(child))
                {
                    continue;
                }

                queue.Enqueue(child);
            }
        }

        return descendants;
    }

    private static IReadOnlyList<ConfigMenuRowViewModel> BuildRows(IReadOnlyList<MenuDto> menus)
    {
        if (menus.Count == 0)
        {
            return [];
        }

        var roots = menus
            .Where(x => x.ParentId is null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();

        var byParent = menus
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList());

        var nameById = menus.ToDictionary(x => x.Id, x => x.Name);
        var rows = new List<ConfigMenuRowViewModel>();

        void AddChildren(int? parentId, int level)
        {
            List<MenuDto> children;

            if (parentId is null)
            {
                children = roots;
            }
            else if (!byParent.TryGetValue(parentId.Value, out children!))
            {
                return;
            }

            foreach (var child in children)
            {
                rows.Add(new ConfigMenuRowViewModel
                {
                    Menu = child,
                    Level = level,
                    ParentName = child.ParentId.HasValue && nameById.TryGetValue(child.ParentId.Value, out var parentName)
                        ? parentName
                        : null
                });

                AddChildren(child.Id, level + 1);
            }
        }

        AddChildren(null, 0);

        var included = rows.Select(x => x.Menu.Id).ToHashSet();
        foreach (var dangling in menus.Where(x => !included.Contains(x.Id)).OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
        {
            rows.Add(new ConfigMenuRowViewModel
            {
                Menu = dangling,
                Level = 0,
                ParentName = dangling.ParentId.HasValue && nameById.TryGetValue(dangling.ParentId.Value, out var parentName)
                    ? parentName
                    : null
            });
        }

        return rows;
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}






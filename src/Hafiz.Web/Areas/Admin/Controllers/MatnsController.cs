using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MatnsController : Controller
{
    private readonly IMatnService _matnService;

    public MatnsController(IMatnService matnService)
    {
        _matnService = matnService;
    }

    private Guid? GetInstituteId()
    {
        var claim = User.FindFirstValue("InstituteId");
        if (Guid.TryParse(claim, out var id))
            return id;

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var userRepo = HttpContext.RequestServices.GetService<Hafiz.Repositories.Interfaces.IUserRepository>();
            var user = userRepo?.GetByIdAsync(userId).GetAwaiter().GetResult();
            return user?.InstituteId;
        }

        return null;
    }

    // GET: Admin/Matns?category=...&search=...
    public async Task<IActionResult> Index(Hafiz.Domain.Enums.MatnCategory? category = null, string? search = null)
    {
        var instituteId = GetInstituteId();
        var matns = await _matnService.GetAllAvailableAsync(instituteId, category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            matns = matns.Where(m =>
                m.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (m.Author != null && m.Author.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        ViewBag.SelectedCategory = category;
        ViewBag.Search = search;
        return View(matns);
    }

    // GET: Admin/Matns/Create
    public IActionResult Create()
    {
        return View(new CreateMatnDto());
    }

    // POST: Admin/Matns/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMatnDto dto)
    {
        var instituteId = GetInstituteId();
        if (!ModelState.IsValid)
            return View(dto);

        var (success, message, _) = await _matnService.CreateAsync(dto, instituteId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(dto);
        }

        TempData["SuccessMessage"] = "تمت إضافة المتن إلى مكتبة المعهد بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Matns/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var instituteId = GetInstituteId();
        var (success, message) = await _matnService.DeleteAsync(id, instituteId);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }
}

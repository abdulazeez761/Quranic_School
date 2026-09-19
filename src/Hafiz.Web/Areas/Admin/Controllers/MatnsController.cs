using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MatnsController : Controller
{
    private readonly IMatnService _matnService;
    private readonly IStudyProgramService _programService;

    public MatnsController(IMatnService matnService, IStudyProgramService programService)
    {
        _matnService = matnService;
        _programService = programService;
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

    // GET: Admin/Matns/Create?studyProgramId=...
    public async Task<IActionResult> Create(Guid? studyProgramId = null)
    {
        var instituteId = GetInstituteId();
        if (instituteId.HasValue)
        {
            var programs = await _programService.GetAllByInstituteAsync(instituteId.Value, Hafiz.Domain.Enums.ProgramType.Matn);
            ViewBag.Programs = programs.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = studyProgramId.HasValue && p.Id == studyProgramId.Value
            }).ToList();
        }

        return View(new CreateMatnDto { StudyProgramId = studyProgramId });
    }

    // POST: Admin/Matns/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMatnDto dto)
    {
        var instituteId = GetInstituteId();
        if (!ModelState.IsValid)
        {
            if (instituteId.HasValue)
            {
                var programs = await _programService.GetAllByInstituteAsync(instituteId.Value, Hafiz.Domain.Enums.ProgramType.Matn);
                ViewBag.Programs = programs.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = dto.StudyProgramId.HasValue && p.Id == dto.StudyProgramId.Value
                }).ToList();
            }
            return View(dto);
        }

        var (success, message, _) = await _matnService.CreateAsync(dto, instituteId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            if (instituteId.HasValue)
            {
                var programs = await _programService.GetAllByInstituteAsync(instituteId.Value, Hafiz.Domain.Enums.ProgramType.Matn);
                ViewBag.Programs = programs.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = dto.StudyProgramId.HasValue && p.Id == dto.StudyProgramId.Value
                }).ToList();
            }
            return View(dto);
        }

        TempData["SuccessMessage"] = "تمت إضافة المتن إلى مكتبة المعهد بنجاح.";
        if (dto.StudyProgramId.HasValue)
        {
            return RedirectToAction("Edit", "StudyPrograms", new { id = dto.StudyProgramId.Value });
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Matns/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var matn = await _matnService.GetByIdAsync(id);
        if (matn == null)
            return NotFound();

        var instituteId = GetInstituteId();
        if (instituteId.HasValue)
        {
            var programs = await _programService.GetAllByInstituteAsync(instituteId.Value, Hafiz.Domain.Enums.ProgramType.Matn);
            ViewBag.Programs = programs.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = matn.StudyProgramId.HasValue && p.Id == matn.StudyProgramId.Value
            }).ToList();
        }

        var dto = new UpdateMatnDto
        {
            Id = matn.Id,
            Title = matn.Title,
            Author = matn.Author,
            Category = matn.Category,
            TotalVerses = matn.TotalVerses,
            TotalChapters = matn.TotalChapters,
            DefaultUnit = matn.DefaultUnit,
            StudyProgramId = matn.StudyProgramId,
            Order = matn.Order,
            PassingGrade = matn.PassingGrade > 0 ? matn.PassingGrade : 60m,
            IsActive = matn.IsActive
        };

        return View(dto);
    }

    // POST: Admin/Matns/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateMatnDto dto)
    {
        var instituteId = GetInstituteId();
        if (!ModelState.IsValid)
            return View(dto);

        var (success, message) = await _matnService.UpdateAsync(dto.Id, dto, instituteId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(dto);
        }

        TempData["SuccessMessage"] = message;
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

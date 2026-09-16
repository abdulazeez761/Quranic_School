using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.StudyProgram;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StudyProgramsController : Controller
{
    private readonly IStudyProgramService _programService;
    private readonly IMatnService _matnService;

    public StudyProgramsController(IStudyProgramService programService, IMatnService matnService)
    {
        _programService = programService;
        _matnService = matnService;
    }

    private Guid? GetInstituteId()
    {
        var claim = User.FindFirstValue("InstituteId");
        return claim != null ? Guid.Parse(claim) : null;
    }

    // GET: Admin/StudyPrograms
    public async Task<IActionResult> Index(ProgramType? type = null)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var programs = await _programService.GetAllByInstituteAsync(instituteId.Value, type);
        ViewBag.SelectedType = type;
        return View(programs);
    }

    // GET: Admin/StudyPrograms/Create
    public async Task<IActionResult> Create()
    {
        await PopulateMatnsDropdown();
        return View(new CreateStudyProgramDto { Type = ProgramType.Quran });
    }

    // POST: Admin/StudyPrograms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudyProgramDto dto)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            await PopulateMatnsDropdown(dto.MatnId);
            return View(dto);
        }

        var (success, message, _) = await _programService.CreateAsync(dto, instituteId.Value);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await PopulateMatnsDropdown(dto.MatnId);
            return View(dto);
        }

        TempData["SuccessMessage"] = "تم إنشاء البرنامج التعليمي بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/StudyPrograms/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var program = await _programService.GetByIdAsync(id, instituteId.Value);
        if (program == null)
            return NotFound();

        var dto = new UpdateStudyProgramDto
        {
            Id = program.Id,
            Name = program.Name,
            Description = program.Description,
            Type = program.Type,
            MatnId = program.MatnId,
            IsActive = program.IsActive
        };

        await PopulateMatnsDropdown(dto.MatnId);
        return View(dto);
    }

    // POST: Admin/StudyPrograms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateStudyProgramDto dto)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            await PopulateMatnsDropdown(dto.MatnId);
            return View(dto);
        }

        var (success, message) = await _programService.UpdateAsync(dto, instituteId.Value);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await PopulateMatnsDropdown(dto.MatnId);
            return View(dto);
        }

        TempData["SuccessMessage"] = "تم تحديث البرنامج التعليمي بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/StudyPrograms/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var (success, message) = await _programService.DeleteAsync(id, instituteId.Value);
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMatnsDropdown(Guid? selectedMatnId = null)
    {
        var instituteId = GetInstituteId();
        var matns = await _matnService.GetAllAvailableAsync(instituteId);
        ViewBag.Matns = matns.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{m.Title} {(string.IsNullOrEmpty(m.Author) ? "" : $"({m.Author})")}",
            Selected = selectedMatnId.HasValue && m.Id == selectedMatnId.Value
        }).ToList();
    }
}

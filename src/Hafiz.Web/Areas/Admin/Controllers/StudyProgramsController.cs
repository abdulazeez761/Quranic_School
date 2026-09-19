using System;
using System.Collections.Generic;
using System.Linq;
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
        await PopulateAvailableMatns();
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
            await PopulateAvailableMatns();
            return View(dto);
        }

        var (success, message, createdId) = await _programService.CreateAsync(dto, instituteId.Value);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await PopulateAvailableMatns();
            return View(dto);
        }

        TempData["SuccessMessage"] = "تم إنشاء البرنامج التعليمي بنجاح.";
        if (dto.Type == ProgramType.Matn && createdId.HasValue)
        {
            return RedirectToAction(nameof(Edit), new { id = createdId.Value });
        }

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
            IsActive = program.IsActive
        };

        ViewBag.Program = program;
        await PopulateAvailableMatnsForProgram(id);
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
            var program = await _programService.GetByIdAsync(dto.Id, instituteId.Value);
            ViewBag.Program = program;
            await PopulateAvailableMatnsForProgram(dto.Id);
            return View(dto);
        }

        var (success, message) = await _programService.UpdateAsync(dto, instituteId.Value);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            var program = await _programService.GetByIdAsync(dto.Id, instituteId.Value);
            ViewBag.Program = program;
            await PopulateAvailableMatnsForProgram(dto.Id);
            return View(dto);
        }

        TempData["SuccessMessage"] = "تم تحديث البرنامج التعليمي بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/StudyPrograms/AddMatn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMatn(Guid programId, Guid matnId)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var (success, message) = await _programService.AddMatnToProgramAsync(programId, matnId, instituteId.Value);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Edit), new { id = programId });
    }

    // POST: Admin/StudyPrograms/RemoveMatn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMatn(Guid programId, Guid matnId)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var (success, message) = await _programService.RemoveMatnFromProgramAsync(programId, matnId, instituteId.Value);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Edit), new { id = programId });
    }

    // POST: Admin/StudyPrograms/MoveMatnUp
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveMatnUp(Guid programId, Guid matnId)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var program = await _programService.GetByIdAsync(programId, instituteId.Value);
        if (program != null)
        {
            var matuns = program.Matuns.OrderBy(m => m.Order).ToList();
            var index = matuns.FindIndex(m => m.Id == matnId);
            if (index > 0)
            {
                (matuns[index - 1], matuns[index]) = (matuns[index], matuns[index - 1]);
                await _programService.ReorderMatunsAsync(programId, matuns.Select(m => m.Id).ToList(), instituteId.Value);
                TempData["SuccessMessage"] = "تم تحديث ترتيب المتون بنجاح.";
            }
        }

        return RedirectToAction(nameof(Edit), new { id = programId });
    }

    // POST: Admin/StudyPrograms/MoveMatnDown
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveMatnDown(Guid programId, Guid matnId)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var program = await _programService.GetByIdAsync(programId, instituteId.Value);
        if (program != null)
        {
            var matuns = program.Matuns.OrderBy(m => m.Order).ToList();
            var index = matuns.FindIndex(m => m.Id == matnId);
            if (index >= 0 && index < matuns.Count - 1)
            {
                (matuns[index + 1], matuns[index]) = (matuns[index], matuns[index + 1]);
                await _programService.ReorderMatunsAsync(programId, matuns.Select(m => m.Id).ToList(), instituteId.Value);
                TempData["SuccessMessage"] = "تم تحديث ترتيب المتون بنجاح.";
            }
        }

        return RedirectToAction(nameof(Edit), new { id = programId });
    }

    // POST: Admin/StudyPrograms/ToggleMatnActive
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMatnActive(Guid programId, Guid matnId)
    {
        var instituteId = GetInstituteId();
        if (!instituteId.HasValue)
            return Unauthorized();

        var (success, message) = await _programService.ToggleMatnActiveAsync(programId, matnId, instituteId.Value);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Edit), new { id = programId });
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
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAvailableMatns()
    {
        var instituteId = GetInstituteId();
        var matns = await _matnService.GetUnassignedLibraryMatnsAsync(instituteId);
        ViewBag.AvailableMatns = matns.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{m.Title} {(string.IsNullOrEmpty(m.Author) ? "" : $"({m.Author})")} - {m.CategoryName}"
        }).ToList();
    }

    private async Task PopulateAvailableMatnsForProgram(Guid programId)
    {
        var instituteId = GetInstituteId();
        var allMatns = await _matnService.GetAllAvailableAsync(instituteId);
        // exclude matns already in this program
        var availableMatns = allMatns.Where(m => m.StudyProgramId != programId).ToList();

        ViewBag.AvailableMatns = availableMatns.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{m.Title} {(string.IsNullOrEmpty(m.Author) ? "" : $"({m.Author})")} - {m.CategoryName}" + (m.StudyProgramId.HasValue ? $" [مرتبط بـ: {m.StudyProgramName}]" : "")
        }).ToList();
    }
}

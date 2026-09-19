using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher,Admin,SuperAdmin")]
public class StudentMatnProgressController : Controller
{
    private readonly IStudentMatnProgressService _progressService;
    private readonly IClassService _classService;
    private readonly ITeacherService _teacherService;

    public StudentMatnProgressController(
        IStudentMatnProgressService progressService,
        IClassService classService,
        ITeacherService teacherService)
    {
        _progressService = progressService;
        _classService = classService;
        _teacherService = teacherService;
    }

    private Guid GetTeacherId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private bool IsAdminUser() => User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

    // GET: Teacher/StudentMatnProgress/ForStudent?studentId=...
    [HttpGet]
    public async Task<IActionResult> ForStudent(Guid studentId)
    {
        var progresses = await _progressService.GetByStudentAsync(studentId);
        return Json(new { success = true, data = progresses });
    }

    // GET: Teacher/StudentMatnProgress/ClassProgress?classId=...
    [HttpGet]
    public async Task<IActionResult> ClassProgress(Guid classId)
    {
        var teacherId = GetTeacherId();
        if (!IsAdminUser() && teacherId == Guid.Empty)
            return Unauthorized();

        var cls = await _classService.GetClassById(classId);
        if (cls == null)
            return NotFound();

        var progresses = await _progressService.GetByClassAsync(classId);
        ViewBag.Class = cls;
        return View(progresses);
    }

    // POST: Teacher/StudentMatnProgress/CompleteStudy
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CompleteStudy(Guid id)
    {
        var isAdmin = IsAdminUser();
        var userId = GetTeacherId();
        if (!isAdmin && userId == Guid.Empty)
            return Json(new { success = false, message = "غير مصرح" });

        var teacherIdForAuth = isAdmin ? Guid.Empty : userId;
        var (success, message) = await _progressService.CompleteStudyAsync(id, teacherIdForAuth);
        return Json(new { success, message });
    }

    // POST: Teacher/StudentMatnProgress/CompleteMemorization
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CompleteMemorization(Guid id)
    {
        var isAdmin = IsAdminUser();
        var userId = GetTeacherId();
        if (!isAdmin && userId == Guid.Empty)
            return Json(new { success = false, message = "غير مصرح" });

        var teacherIdForAuth = isAdmin ? Guid.Empty : userId;
        var (success, message) = await _progressService.CompleteMemorizationAsync(id, teacherIdForAuth);
        return Json(new { success, message });
    }

    // POST: Teacher/StudentMatnProgress/CompleteBoth
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CompleteBoth([FromBody] CompleteBothStudyAndMemorizationDto dto)
    {
        var isAdmin = IsAdminUser();
        var userId = GetTeacherId();
        if (!isAdmin && userId == Guid.Empty)
            return Json(new { success = false, message = "غير مصرح" });

        var teacherIdForAuth = isAdmin ? Guid.Empty : userId;
        var (success, message) = await _progressService.CompleteStudyAndMemorizationAsync(dto, teacherIdForAuth);
        return Json(new { success, message });
    }

    // POST: Teacher/StudentMatnProgress/RecordExam
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RecordExam([FromBody] RecordExamResultDto dto)
    {
        var isAdmin = IsAdminUser();
        var userId = GetTeacherId();
        if (!isAdmin && userId == Guid.Empty)
            return Json(new { success = false, message = "غير مصرح" });

        var teacherIdForAuth = isAdmin ? Guid.Empty : userId;
        var (success, message) = await _progressService.RecordExamResultAsync(dto, teacherIdForAuth);
        return Json(new { success, message });
    }

    // POST: Teacher/StudentMatnProgress/CreateOrUpdate
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateOrUpdate([FromBody] CreateStudentMatnProgressDto dto)
    {
        var isAdmin = IsAdminUser();
        var userId = GetTeacherId();
        if (!isAdmin && userId == Guid.Empty)
            return Json(new { success = false, message = "غير مصرح" });

        var teacherIdForAuth = isAdmin ? Guid.Empty : userId;
        var (success, message, id) = await _progressService.CreateOrUpdateAsync(dto, teacherIdForAuth);
        return Json(new { success, message, id });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.Matn;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class MatnController : Controller
{
    private readonly IMatnAssignmentService _assignmentService;
    private readonly IClassService _classService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly IUserRepository _userRepository;

    public MatnController(
        IMatnAssignmentService assignmentService,
        IClassService classService,
        IStudentService studentService,
        ITeacherService teacherService,
        IUserRepository userRepository)
    {
        _assignmentService = assignmentService;
        _classService = classService;
        _studentService = studentService;
        _teacherService = teacherService;
        _userRepository = userRepository;
    }

    private Guid GetTeacherId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private async Task<Guid?> GetInstituteIdAsync(Guid teacherId)
    {
        var claim = User.FindFirstValue("InstituteId");
        if (Guid.TryParse(claim, out var id))
            return id;

        if (teacherId != Guid.Empty)
        {
            var user = await _userRepository.GetByIdAsync(teacherId);
            return user?.InstituteId;
        }

        return null;
    }

    // GET: Teacher/Matn?classId=...&date=...
    public async Task<IActionResult> Index(Guid? classId = null, DateTime? date = null)
    {
        var teacherId = GetTeacherId();
        if (teacherId == Guid.Empty)
            return Unauthorized();

        var instituteId = await GetInstituteIdAsync(teacherId);
        if (!instituteId.HasValue)
            return Unauthorized();

        var teacherClasses = (await _teacherService.GetTeacherClasses(teacherId)) ?? new List<Class>();
        ViewBag.TeacherClasses = teacherClasses;

        if (!teacherClasses.Any())
        {
            ViewBag.NoClasses = true;
            return View(new List<MatnAssignmentDto>());
        }

        Class? selectedClass = null;

        // 1. If explicit classId requested, verify teacher teaches it
        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            selectedClass = teacherClasses.FirstOrDefault(c => c.Id == classId.Value);
        }

        // 2. Try cookie
        if (selectedClass == null)
        {
            var cookieClassId = Request.Cookies["selectedClassId"];
            if (Guid.TryParse(cookieClassId, out var parsedId))
            {
                selectedClass = teacherClasses.FirstOrDefault(c => c.Id == parsedId);
            }
        }

        // 3. Fallback: prefer class with Matn program, then first class
        if (selectedClass == null)
        {
            selectedClass = teacherClasses.FirstOrDefault(c => c.StudyProgram != null && c.StudyProgram.Type == ProgramType.Matn)
                            ?? teacherClasses.FirstOrDefault();
        }

        if (selectedClass == null)
        {
            ViewBag.NoClasses = true;
            return View(new List<MatnAssignmentDto>());
        }

        // Save selected class to cookie for smooth subsequent requests
        Response.Cookies.Append("selectedClassId", selectedClass.Id.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });

        var cls = await _classService.GetClassById(selectedClass.Id, instituteId.Value);
        if (cls == null)
        {
            ViewBag.NoClasses = true;
            return View(new List<MatnAssignmentDto>());
        }

        var studentsList = new List<SelectListItem>();
        if (cls.StudentsIds != null)
        {
            foreach (var sId in cls.StudentsIds)
            {
                var s = await _studentService.GetByIdAsync(sId, instituteId.Value);
                if (s != null)
                {
                    studentsList.Add(new SelectListItem
                    {
                        Value = sId.ToString(),
                        Text = $"{s.FirstName} {s.SecondName}"
                    });
                }
            }
        }
        ViewBag.Students = studentsList;

        var targetDate = date ?? DateTime.Today;
        var assignments = await _assignmentService.GetByClassAndDateAsync(selectedClass.Id, targetDate);

        ViewBag.Class = cls;
        ViewBag.TargetDate = targetDate;

        return View(assignments);
    }

    // POST: Teacher/Matn/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignMatnDto dto)
    {
        var teacherId = GetTeacherId();
        if (teacherId == Guid.Empty)
            return Unauthorized();

        var instituteId = await GetInstituteIdAsync(teacherId);
        if (!instituteId.HasValue)
            return Unauthorized();

        var teacherClasses = await _teacherService.GetTeacherClasses(teacherId);
        if (teacherClasses == null || !teacherClasses.Any(c => c.Id == dto.ClassId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "يرجى التحقق من صحة البيانات المدخلة.";
            return RedirectToAction(nameof(Index), new { classId = dto.ClassId, date = dto.AssignedDate?.ToString("yyyy-MM-dd") });
        }

        var (success, message, _) = await _assignmentService.AssignAsync(dto, teacherId);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index), new { classId = dto.ClassId, date = dto.AssignedDate?.ToString("yyyy-MM-dd") });
    }

    // POST: Teacher/Matn/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateMatnStatusDto dto, Guid classId, DateTime? date = null)
    {
        var teacherId = GetTeacherId();
        if (teacherId == Guid.Empty)
            return Unauthorized();

        var instituteId = await GetInstituteIdAsync(teacherId);
        if (!instituteId.HasValue)
            return Unauthorized();

        var teacherClasses = await _teacherService.GetTeacherClasses(teacherId);
        if (teacherClasses == null || !teacherClasses.Any(c => c.Id == classId))
            return Forbid();

        var (success, message) = await _assignmentService.UpdateStatusAsync(dto, teacherId);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index), new { classId = classId, date = date?.ToString("yyyy-MM-dd") });
    }

    // POST: Teacher/Matn/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid classId, DateTime? date = null)
    {
        var teacherId = GetTeacherId();
        if (teacherId == Guid.Empty)
            return Unauthorized();

        var instituteId = await GetInstituteIdAsync(teacherId);
        if (!instituteId.HasValue)
            return Unauthorized();

        var teacherClasses = await _teacherService.GetTeacherClasses(teacherId);
        if (teacherClasses == null || !teacherClasses.Any(c => c.Id == classId))
            return Forbid();

        var (success, message) = await _assignmentService.DeleteAsync(id, teacherId);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index), new { classId = classId, date = date?.ToString("yyyy-MM-dd") });
    }

    // GET: Teacher/Matn/ExportExcel?classId=...&date=...
    [HttpGet]
    public async Task<IActionResult> ExportExcel(Guid? classId = null, DateTime? date = null)
    {
        var teacherId = GetTeacherId();
        if (teacherId == Guid.Empty)
            return Unauthorized();

        var instituteId = await GetInstituteIdAsync(teacherId);
        if (!instituteId.HasValue)
            return Unauthorized();

        var teacherClasses = (await _teacherService.GetTeacherClasses(teacherId)) ?? new List<Class>();
        Class? targetClass = null;
        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            targetClass = teacherClasses.FirstOrDefault(c => c.Id == classId.Value);
        }
        if (targetClass == null)
        {
            var cookieClassId = Request.Cookies["selectedClassId"];
            if (Guid.TryParse(cookieClassId, out var parsedId))
                targetClass = teacherClasses.FirstOrDefault(c => c.Id == parsedId);
        }
        if (targetClass == null)
        {
            targetClass = teacherClasses.FirstOrDefault();
        }

        if (targetClass == null)
            return RedirectToAction(nameof(Index));

        var targetDate = date ?? DateTime.Today;
        var assignments = await _assignmentService.GetByClassAndDateAsync(targetClass.Id, targetDate);

        var bytes = Hafiz.Web.Reporting.MatnReportExcelExporter.Build(assignments);
        var fileName = $"matn-report-{targetClass.Name}-{targetDate:yyyy-MM-dd}.xlsx";
        return File(bytes, Hafiz.Web.Reporting.MatnReportExcelExporter.ContentType, fileName);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Common.Helper;
using Hafiz.DTOs;
using Hafiz.DTOs.Student;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly IParentService _parentService;
        private readonly IParentNoteService _parentNoteService;

        public StudentsController(
            IAuthService authService,
            IStudentService studentService,
            IClassService classService,
            IParentService parentService,
            IParentNoteService parentNoteService
        )
        {
            _authService = authService;
            _studentService = studentService;
            _classService = classService;
            _parentService = parentService;
            _parentNoteService = parentNoteService;
        }

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instituteId = GetInstituteId();
            IEnumerable<StudentModel> students;

            if (instituteId.HasValue)
                students = await _studentService.GetAllByInstituteAsync(instituteId.Value);
            else
                students = await _studentService.GetAllAsync();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            Guid id,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? type = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? tab = null)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            bool? isCompleted = status?.ToLower() switch
            {
                "completed" => true,
                "pending" => false,
                _ => null,
            };

            AssignmentType? assignmentType = null;
            if (!string.IsNullOrEmpty(type) && Enum.TryParse(type, true, out AssignmentType parsedType))
                assignmentType = parsedType;

            var paginatedWirds = await _studentService.GetStudentWirdsPaginatedAsync(
                id, page, pageSize, isCompleted, assignmentType);

            var attendance = await _studentService.GetStudentAttendanceAsync(id);
            var attendanceList = attendance.ToList();

            if (fromDate.HasValue)
                attendanceList = attendanceList.Where(a => a.Date >= fromDate.Value).ToList();
            if (toDate.HasValue)
                attendanceList = attendanceList.Where(a => a.Date <= toDate.Value).ToList();

            int totalAtt = attendanceList.Count;
            int presentCount = attendanceList.Count(a => a.Status == AttendanceStatus.Present);
            int lateCount = attendanceList.Count(a => a.Status == AttendanceStatus.Late);
            int absentCount = attendanceList.Count(a => a.Status == AttendanceStatus.Absent);
            int excusedCount = attendanceList.Count(a => a.Status == AttendanceStatus.Excused);
            double attRate = totalAtt > 0
                ? Math.Round((double)(presentCount + lateCount) / totalAtt * 100, 1)
                : 0;

            var notes = await _parentNoteService.GetNotesByStudentIdAsync(id);

            var viewModel = new AdminStudentDetailsViewModel
            {
                Student = student,
                PaginatedWirds = paginatedWirds,
                Attendance = attendanceList,
                AttendanceRate = attRate,
                PresentCount = presentCount,
                LateCount = lateCount,
                AbsentCount = absentCount,
                ExcusedCount = excusedCount,
                ParentNotes = notes,
                WirdStatus = status,
                WirdType = type,
                AttendanceFromDate = fromDate,
                AttendanceToDate = toDate,
                ActiveTab = tab ?? "overview",
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Reports(Guid? classId = null, string? sortBy = null, string? sortOrder = null)
        {
            var instituteId = GetInstituteId();
            IEnumerable<StudentModel> students;

            if (instituteId.HasValue)
                students = await _studentService.GetAllByInstituteAsync(instituteId.Value);
            else
                students = await _studentService.GetAllAsync();

            if (classId.HasValue)
                students = students.Where(s => s.Classes.Any(c => c.Id == classId.Value));

            var reportRows = new List<StudentReportRow>();
            foreach (var student in students)
            {
                var attendance = await _studentService.GetStudentAttendanceAsync(student.UserId);
                var attendanceList = attendance.ToList();
                var wirds = await _studentService.GetStudentWirdsAsync(student.UserId);
                var wirdsList = wirds.ToList();

                int totalAtt = attendanceList.Count;
                int presentCount = attendanceList.Count(a =>
                    a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);

                var totalMemorizedPages = WirdPageCalculator.TotalMemorizedPages(student);
                var (juz, _) = WirdPageCalculator.SplitJuzAndPages(totalMemorizedPages);

                reportRows.Add(new StudentReportRow
                {
                    StudentId = student.UserId,
                    FullName = $"{student.StudentInfo.FirstName} {student.StudentInfo.SecondName}",
                    ClassName = string.Join(", ", student.Classes.Select(c => c.Name)),
                    TotalMemorizedPages = totalMemorizedPages,
                    MemorizedJuz = juz,
                    ReviewedPages = student.ReviewedPages,
                    TotalWirds = wirdsList.Count,
                    CompletedWirds = wirdsList.Count(w => w.IsCompleted),
                    AttendanceRate = totalAtt > 0 ? Math.Round((double)presentCount / totalAtt * 100, 1) : 0,
                    TotalAttendance = totalAtt,
                    IsHafiz = WirdPageCalculator.IsHafiz(student),
                    TajwidLevel = student.TajwidLevel,
                });
            }

            reportRows = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("memorization", "asc") => reportRows.OrderBy(r => r.TotalMemorizedPages).ToList(),
                ("memorization", _) => reportRows.OrderByDescending(r => r.TotalMemorizedPages).ToList(),
                ("attendance", "asc") => reportRows.OrderBy(r => r.AttendanceRate).ToList(),
                ("attendance", _) => reportRows.OrderByDescending(r => r.AttendanceRate).ToList(),
                ("name", "desc") => reportRows.OrderByDescending(r => r.FullName).ToList(),
                ("name", _) => reportRows.OrderBy(r => r.FullName).ToList(),
                _ => reportRows.OrderByDescending(r => r.TotalMemorizedPages).ToList(),
            };

            await PopulateClassesDropdown();

            var viewModel = new AdminStudentReportsViewModel
            {
                Students = reportRows,
                ClassFilter = classId,
                SortBy = sortBy,
                SortOrder = sortOrder,
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateClassesDropdown();
            await PopulateParentsDropdown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterStudentDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(registerDto);
            }
            var instituteId = GetInstituteId();
            var (Success, ErrorMessage) = await _studentService.AddAsync(registerDto, instituteId);
            if (!Success)
            {
                ModelState.AddModelError("", ErrorMessage);
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(registerDto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _studentService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student is null)
                return NotFound();
            EditStudentDto editStudentDto = new EditStudentDto
            {
                StudentID = id,
                FirstName = student.FirstName,
                SecondName = student.SecondName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                ClassId = student.ClassId,
                ParentId = student.ParentId,
                MemorizedJuz = student.MemorizedJuz,
                TajwidLevel = student.TajwidLevel,
                sex = student.sex,
                ClassesIds = student.ClassesIds,
            };

            await PopulateParentsDropdown();
            await PopulateClassesDropdown();
            return View(editStudentDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditStudentDto newData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateClassesDropdown();
                    await PopulateParentsDropdown();
                    return View(newData);
                }
                await _studentService.UpdateAsync(newData);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(newData);
            }
        }

        private async Task PopulateClassesDropdown()
        {
            var instituteId = GetInstituteId();
            IEnumerable<ClassDto> classes;

            if (instituteId.HasValue)
                classes = await _classService.GetClassesByInstituteAsync(instituteId.Value);
            else
                classes = await _classService.GetClassesAsync();

            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
        }

        private async Task PopulateParentsDropdown()
        {
            var instituteId = GetInstituteId();
            IEnumerable<Hafiz.Models.Parent> parents;

            if (instituteId.HasValue)
                parents = await _parentService.GetAllByInstituteAsync(instituteId.Value);
            else
                parents = await _parentService.GetAllAsync();

            ViewBag.Parents = parents
                .Select(p => new SelectListItem
                {
                    Value = p.UserId.ToString(),
                    Text = $"{p.ParentInfo.FirstName} {p.ParentInfo.SecondName}",
                })
                .ToList();
        }
    }
}

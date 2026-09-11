using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.Extensions;
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
        public async Task<IActionResult> Index(
            bool archived = false,
            int page = 1,
            int pageSize = 12,
            string? search = null,
            Guid? classId = null
        )
        {
            var instituteId = GetInstituteId();
            IEnumerable<StudentModel> students;

            if (archived)
            {
                students = instituteId.HasValue
                    ? await _studentService.GetArchivedByInstituteAsync(instituteId.Value)
                    : await _studentService.GetArchivedAsync();
            }
            else
            {
                students = instituteId.HasValue
                    ? await _studentService.GetAllByInstituteAsync(instituteId.Value)
                    : await _studentService.GetAllAsync();
            }

            var studentList = students.ToList();
            var totalStudents = studentList.Count;
            var totalClasses = studentList.SelectMany(s => s.Classes).Select(c => c.Id).Distinct().Count();
            var studentsWithClasses = studentList.Count(s => s.Classes.Any());
            var studentsWithoutClasses = totalStudents - studentsWithClasses;

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.StudentsWithClasses = studentsWithClasses;
            ViewBag.StudentsWithoutClasses = studentsWithoutClasses;

            if (instituteId.HasValue)
            {
                var classes = await _classService.GetClassesByInstituteAsync(instituteId.Value);
                ViewBag.InstituteClasses = classes.OrderBy(c => c.Name).ToList();
            }

            var filtered = studentList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                filtered = filtered.Where(s =>
                    (s.StudentInfo.FirstName != null && s.StudentInfo.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (s.StudentInfo.SecondName != null && s.StudentInfo.SecondName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (s.StudentInfo.Username != null && s.StudentInfo.Username.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (s.StudentInfo.PhoneNumber != null && s.StudentInfo.PhoneNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (classId.HasValue && classId.Value != Guid.Empty)
            {
                filtered = filtered.Where(s => s.Classes.Any(c => c.Id == classId.Value));
            }

            var pagedStudents = filtered.ToPagedResult(page, pageSize);

            var (activeCount, archivedCount) = await _studentService.GetCountsAsync(instituteId);
            ViewBag.IsArchived = archived;
            ViewBag.ActiveCount = activeCount;
            ViewBag.ArchivedCount = archivedCount;
            ViewBag.Search = search;
            ViewBag.ClassId = classId;

            return View(pagedStudents);
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
            string? tab = null
        )
        {
            var instituteId = GetInstituteId();
            var student = await _studentService.GetStudentByIdAsync(id, instituteId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "تعذر العثور على بيانات الطالب المطلوب.";
                return RedirectToAction(nameof(Index));
            }

            bool? isCompleted = status?.ToLower() switch
            {
                "completed" => true,
                "pending" => false,
                _ => null,
            };

            bool? isUpcoming = status?.ToLower() == "upcoming" ? true : null;

            AssignmentType? assignmentType = null;
            if (
                !string.IsNullOrEmpty(type)
                && Enum.TryParse(type, true, out AssignmentType parsedType)
            )
                assignmentType = parsedType;

            var paginatedWirds = await _studentService.GetStudentWirdsPaginatedAsync(
                id,
                page,
                pageSize,
                isCompleted,
                assignmentType,
                isUpcoming
            );

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
            double attRate =
                totalAtt > 0
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
        public async Task<IActionResult> Reports(
            Guid? classId = null,
            string? search = null,
            string? sortBy = null,
            string? sortOrder = null,
            int page = 1,
            int pageSize = 10
        )
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            var classes = await _classService.GetClassesByInstituteAsync(instituteId.Value);
            ViewBag.Classes = classes
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var reportRows = await _studentService.GetStudentReportsByInstituteAsync(
                instituteId.Value,
                classId,
                search,
                sortBy,
                sortOrder
            );

            var rowsList = reportRows.ToList();
            var pagedRows = rowsList.ToPagedResult(page, pageSize);

            var viewModel = new AdminStudentReportsViewModel
            {
                Students = rowsList,
                PagedStudents = pagedRows,
                SortBy = sortBy,
                SortOrder = sortOrder,
                ClassId = classId,
                Search = search,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNoteAsRead(Guid noteId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _parentNoteService.MarkNoteAsReadAsync(noteId, userId);
                if (!success)
                    return Json(new { success = false, message = "Note not found." });

                return Json(
                    new { success = true, readAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to mark note as read." });
            }
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
            var instituteId = GetInstituteId();
            var deleted = await _studentService.DeleteAsync(id, instituteId);
            if (!deleted)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بحذف هذا الطالب أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت أرشفة الطالب بنجاح، ويمكنك استعادته في أي وقت من قسم الأرشيف.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var instituteId = GetInstituteId();
            var restored = await _studentService.RestoreStudentAsync(id, instituteId);
            if (!restored)
            {
                TempData["ErrorMessage"] = "غير مصرح لك باستعادة هذا الطالب أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت استعادة الطالب بنجاح وإعادة تفعيله.";
            }

            return RedirectToAction(nameof(Index), new { archived = true });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var instituteId = GetInstituteId();
            StudentDto? student = await _studentService.GetByIdAsync(id, instituteId);

            if (student is null)
                return NotFound();
            EditStudentDto editStudentDto = new EditStudentDto
            {
                StudentID = id,
                FirstName = student.FirstName,
                SecondName = student.SecondName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Username = student.Username,
                // Password = student.,
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

                var instituteId = GetInstituteId();
                if (newData.StudentID.HasValue)
                {
                    var existingStudent = await _studentService.GetByIdAsync(newData.StudentID.Value, instituteId);
                    if (existingStudent is null)
                        return Forbid();
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

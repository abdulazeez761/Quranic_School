using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.DTO.StudentWird;
using Hafiz.Application.DTO.Wird;
using Hafiz.Application.Extensions;
using Hafiz.DTOs.Student;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Hafiz.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;
        private readonly IStudentService _studentService;
        private readonly IWirdService _wirdService;
        private readonly IParentNoteService _parentNoteService;
        private readonly IStudentWirdService _studentWirdService;

        public StudentController(
            ILogger<StudentController> logger,
            IStudentService studentService,
            IWirdService wirdService,
            IParentNoteService parentNoteService,
            IStudentWirdService studentWirdService
        )
        {
            _logger = logger;
            _studentService = studentService;
            _wirdService = wirdService;
            _parentNoteService = parentNoteService;
            _studentWirdService = studentWirdService;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 12,
            string? search = null,
            string? level = null
        )
        {
            string? selectedClassFromCookies = Request.Cookies["selectedClassId"];
            ViewBag.ClassId = selectedClassFromCookies;
            Guid? selectedClass;
            if (selectedClassFromCookies is not null)
                selectedClass = Guid.Parse(selectedClassFromCookies);
            else
            {
                ModelState.AddModelError(string.Empty, "");
                return View(new PagedResult<StudentModel>());
            }

            IEnumerable<StudentModel> students = await _studentService.GetStudentsByClassID(
                selectedClass
            );

            var studentList = students.ToList();
            var totalStudents = studentList.Count;
            var boysCount = studentList.Count(s => s.sex == Hafiz.Models.enums.Sex.male);
            var girlsCount = studentList.Count(s => s.sex == Hafiz.Models.enums.Sex.female);
            var className = studentList
                .FirstOrDefault()
                ?.Classes.FirstOrDefault(c => c.Id == selectedClass)
                ?.Name;

            ViewBag.TotalStudents = totalStudents;
            ViewBag.BoysCount = boysCount;
            ViewBag.GirlsCount = girlsCount;
            ViewBag.ClassName = className;
            ViewBag.Search = search;
            ViewBag.Level = level;
            ViewBag.AllClassStudents = studentList
                .Select(s =>
                {
                    var fn = s.StudentInfo?.FirstName ?? "";
                    var sn = s.StudentInfo?.SecondName ?? "";
                    var fullName = $"{fn} {sn}".Trim();
                    var initials = (
                        (fn.Length > 0 ? fn[0].ToString() : "")
                        + (sn.Length > 0 ? sn[0].ToString() : "")
                    ).ToUpper();
                    return new
                    {
                        id = s.UserId.ToString(),
                        name = fullName,
                        initials = initials,
                        level = s.TajwidLevel.ToString(),
                    };
                })
                .ToList();

            var filtered = studentList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                filtered = filtered.Where(s =>
                    (
                        s.StudentInfo.FirstName != null
                        && s.StudentInfo.FirstName.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    || (
                        s.StudentInfo.SecondName != null
                        && s.StudentInfo.SecondName.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    || (
                        s.StudentInfo.Username != null
                        && s.StudentInfo.Username.Contains(term, StringComparison.OrdinalIgnoreCase)
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(level) && level != "all")
            {
                filtered = filtered.Where(s =>
                    string.Equals(
                        s.TajwidLevel.ToString(),
                        level,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            var pagedStudents = filtered.ToPagedResult(page, pageSize);

            return View(pagedStudents);
        }

        public async Task<IActionResult> Details(Guid id, int page = 1)
        {
            const int pageSize = 5;

            try
            {
                StudentModel? student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "تعذر العثور على بيانات الطالب المطلوب.";
                    return RedirectToAction("Index");
                }

                // Get parent notes for this student
                var parentNotes = await _parentNoteService.GetNotesByStudentIdAsync(id);
                ViewBag.ParentNotes = parentNotes;

                // Calculate pagination
                int totalWirds = student.wirds?.Count ?? 0;
                int totalPages = (int)Math.Ceiling((double)totalWirds / pageSize);

                // Validate page number
                if (page < 1)
                    page = 1;
                if (page > totalPages && totalPages > 0)
                    page = totalPages;

                // Get paginated Wirds
                List<WirdAssignment> paginatedWirds =
                    student
                        .wirds?.OrderByDescending(w => w.AssignedDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList() ?? new();

                // Create view model
                var viewModel = new StudentDetailsViewModel
                {
                    Student = student,
                    PaginatedWirds = paginatedWirds,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalWirds = totalWirds,
                    PageSize = pageSize,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student details for ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while retrieving student details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignWirdsBatch([FromBody] AssignWirdsBatchDto model)
        {
            // Navigation properties are EF relations not supplied in batch DTO
            foreach (
                var key in ModelState
                    .Keys.Where(k =>
                        k.EndsWith(".Student")
                        || k.EndsWith(".Class")
                        || k == "Student"
                        || k == "Class"
                    )
                    .ToList()
            )
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(
                    " | ",
                    ModelState
                        .Values.SelectMany(v => v.Errors)
                        .Select(e =>
                            !string.IsNullOrEmpty(e.ErrorMessage)
                                ? e.ErrorMessage
                                : e.Exception?.Message
                        )
                );
                _logger.LogWarning("AssignWirdsBatch invalid ModelState: {Errors}", errors);
                return BadRequest(
                    new { success = false, message = $"بيانات الورد غير صالحة: {errors}" }
                );
            }

            if (model == null || model.Wirds == null || !model.Wirds.Any())
                return BadRequest(new { success = false, message = "لا توجد أوراد لحفظها." });

            // Bind the wird to the teacher's currently selected class. We trust the
            // cookie over the form field so a tampered ClassId can't pin the wird to
            // a class the teacher isn't scoped to.
            if (Guid.TryParse(Request.Cookies["selectedClassId"], out var classId))
                model.ClassId = classId;
            else
                model.ClassId = null;
            model.AssignedDate = TimeZoneHelper.GetUserNow(HttpContext);

            (bool isAdded, string message) = await _wirdService.AddWirdAsync(model);

            if (isAdded)
            {
                TempData["SuccessMessage"] = message;
                return Json(new { success = true, message });
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return BadRequest(new { success = false, message });
            }
        }

        /// <summary>
        /// استرجاع سياق أوراد الطالب مباشرة من قاعدة البيانات:
        /// بيانات الطالب، الخطة المعتمدة، أحدث أوراد أنجزها الطالب، وأوراد اليوم إن وجدت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStudentWirdContext(Guid studentId)
        {
            if (studentId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "معرف الطالب غير صالح." });
            }

            try
            {
                var result = await _studentWirdService.GetStudentWirdContextAsync(studentId);

                if (result == null)
                {
                    return NotFound(
                        new { success = false, message = "تعذر العثور على بيانات الطالب." }
                    );
                }

                return Json(StudentWirdContextResponseDto.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting wird context for student {StudentId}",
                    studentId
                );
                return Json(
                    new { success = false, message = "حدث خطأ أثناء جلب بيانات الورد للطالب." }
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClassStudentsForDrawer(Guid? classId)
        {
            if (
                !classId.HasValue
                && Guid.TryParse(Request.Cookies["selectedClassId"], out var parsedClassId)
            )
            {
                classId = parsedClassId;
            }

            if (!classId.HasValue)
                return BadRequest(new { success = false, message = "لم يتم تحديد حلقة." });

            var students = await _studentService.GetStudentsByClassID(classId.Value);
            var list = students
                .Select(s =>
                {
                    var fn = s.StudentInfo?.FirstName ?? "";
                    var sn = s.StudentInfo?.SecondName ?? "";
                    var fullName = $"{fn} {sn}".Trim();
                    var initials = (
                        (fn.Length > 0 ? fn[0].ToString() : "")
                        + (sn.Length > 0 ? sn[0].ToString() : "")
                    ).ToUpper();
                    return new
                    {
                        id = s.UserId,
                        name = fullName,
                        initials = initials,
                        level = s.TajwidLevel.ToString(),
                    };
                })
                .ToList();

            return Json(new { success = true, data = list });
        }

        [HttpPost]
        public async Task<IActionResult> EditWird(WirdAssignment model)
        {
            (bool isUpdated, string message) = await _wirdService.UpdateWirdAsync(model);

            if (isUpdated)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;
            var updatedWird = await _wirdService.GetWirdAssignmentByIdAsync(model.Id);
            return PartialView("_WirdCard", updatedWird);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.DTO.StudentWird;
using Hafiz.Application.DTO.Wird;
using Hafiz.Application.Extensions;
using Hafiz.DTOs.Matn;
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
        private readonly IClassService _classService;
        private readonly IMatnAssignmentService _matnAssignmentService;
        private readonly IStudentMatnProgressService _studentMatnProgressService;

        public StudentController(
            ILogger<StudentController> logger,
            IStudentService studentService,
            IWirdService wirdService,
            IParentNoteService parentNoteService,
            IStudentWirdService studentWirdService,
            IClassService classService,
            IMatnAssignmentService matnAssignmentService,
            IStudentMatnProgressService studentMatnProgressService
        )
        {
            _logger = logger;
            _studentService = studentService;
            _wirdService = wirdService;
            _parentNoteService = parentNoteService;
            _studentWirdService = studentWirdService;
            _classService = classService;
            _matnAssignmentService = matnAssignmentService;
            _studentMatnProgressService = studentMatnProgressService;
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

        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 12,
            string? search = null,
            string? level = null
        )
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            string? selectedClassFromCookies = Request.Cookies["selectedClassId"];
            ViewBag.ClassId = selectedClassFromCookies;
            Guid? selectedClass;
            if (selectedClassFromCookies is not null && Guid.TryParse(selectedClassFromCookies, out var parsedClass))
                selectedClass = parsedClass;
            else
            {
                ModelState.AddModelError(string.Empty, "يجب تحديد الشعبة أولاً");
                return View(new PagedResult<StudentModel>());
            }

            IEnumerable<StudentModel> students = await _studentService.GetStudentsByClassID(
                selectedClass
            );

            var studentList = students.Where(s => s.StudentInfo.InstituteId == instituteId.Value).ToList();
            var totalStudents = studentList.Count;
            var boysCount = studentList.Count(s => s.sex == Hafiz.Models.enums.Sex.male);
            var girlsCount = studentList.Count(s => s.sex == Hafiz.Models.enums.Sex.female);
            var classDto = await _classService.GetClassById(selectedClass.Value, instituteId.Value);
            var className = classDto?.Name
                ?? studentList.FirstOrDefault()?.Classes.FirstOrDefault(c => c.Id == selectedClass)?.Name
                ?? Request.Cookies["selectedClassName"]
                ?? "الشعبة";

            var isMatnClass = (classDto?.ProgramType == Hafiz.Domain.Enums.ProgramType.Matn);
            Dictionary<Guid, (int active, int completed)> matnCountsByStudent = new();
            Dictionary<Guid, int> matnTodayCountsByStudent = new();
            Dictionary<Guid, int> matnUnratedCountsByStudent = new();

            var userToday = TimeZoneHelper.GetUserToday(HttpContext);
            var serverToday = DateTime.Today;
            var utcToday = DateTime.UtcNow.Date;

            if (isMatnClass)
            {
                var classMatnAssignments = await _matnAssignmentService.GetByClassIdAsync(selectedClass.Value);
                matnCountsByStudent = classMatnAssignments
                    .GroupBy(m => m.StudentId)
                    .ToDictionary(
                        g => g.Key,
                        g => (
                            active: g.Count(m => m.Status == AssignmentStatus.notSet),
                            completed: g.Count(m => m.IsCompleted)
                        )
                    );

                matnTodayCountsByStudent = classMatnAssignments
                    .Where(m => m.AssignedDate.Date == userToday
                             || m.AssignedDate.Date == serverToday
                             || m.AssignedDate.Date == utcToday
                             || m.AssignedDate.ToLocalTime().Date == userToday
                             || m.AssignedDate.ToLocalTime().Date == serverToday)
                    .GroupBy(m => m.StudentId)
                    .ToDictionary(g => g.Key, g => g.Count());

                matnUnratedCountsByStudent = classMatnAssignments
                    .Where(m => m.Status == AssignmentStatus.notSet && !m.IsUpcoming)
                    .GroupBy(m => m.StudentId)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            // Quran today counts calculated safely across all timezones
            var quranTodayCountsByStudent = studentList.ToDictionary(
                s => s.UserId,
                s => s.wirds?.Count(w => 
                    w.AssignedDate.Date == userToday 
                    || w.AssignedDate.Date == serverToday 
                    || w.AssignedDate.Date == utcToday 
                    || w.AssignedDate.ToLocalTime().Date == userToday 
                    || w.AssignedDate.ToLocalTime().Date == serverToday) ?? 0
            );

            // Quran unrated counts (status == notSet and not upcoming)
            var quranUnratedCountsByStudent = studentList.ToDictionary(
                s => s.UserId,
                s => s.wirds?.Count(w => w.Status == AssignmentStatus.notSet && !w.IsUpcoming) ?? 0
            );

            ViewBag.TotalStudents = totalStudents;
            ViewBag.BoysCount = boysCount;
            ViewBag.GirlsCount = girlsCount;
            ViewBag.ClassName = className;
            ViewBag.IsMatnClass = isMatnClass;
            ViewBag.MatnCountsByStudent = matnCountsByStudent;
            ViewBag.MatnTodayCountsByStudent = matnTodayCountsByStudent;
            ViewBag.MatnUnratedCountsByStudent = matnUnratedCountsByStudent;
            ViewBag.QuranTodayCountsByStudent = quranTodayCountsByStudent;
            ViewBag.QuranUnratedCountsByStudent = quranUnratedCountsByStudent;
            ViewBag.StudyProgramName = classDto?.StudyProgramName;
            ViewBag.AssignedMatnTitle = classDto?.MatnTitle ?? classDto?.StudyProgramName;
            ViewBag.ClassDto = classDto;
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
                var instituteId = GetInstituteId();
                if (!instituteId.HasValue)
                    return Forbid();

                StudentModel? student = await _studentService.GetStudentByIdAsync(id, instituteId.Value);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "تعذر العثور على بيانات الطالب المطلوب أو غير مصرح لك بعرضه.";
                    return RedirectToAction("Index");
                }

                // Verify teacher has access to this student's class
                var teacherUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                bool hasAccess = student.Classes.Any(c => c.Teachers.Any(t => t.UserId == teacherUserId));
                if (!hasAccess && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                {
                    TempData["ErrorMessage"] = "غير مصرح لك بعرض بيانات طالب خارج شعبك المخصصة.";
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

                // Get student's Matn assignments
                var matnAssignments = (await _matnAssignmentService.GetByStudentIdAsync(id)).ToList();
                var matnProgresses = (await _studentMatnProgressService.GetByStudentAsync(id)).ToList();

                // Create view model
                var viewModel = new StudentDetailsViewModel
                {
                    Student = student,
                    PaginatedWirds = paginatedWirds,
                    MatnAssignments = matnAssignments,
                    MatnProgresses = matnProgresses,
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
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return BadRequest(new { success = false, message = "غير مصرح لك: تعذر تحديد المركز التابع له." });

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

            // Verify student belongs to this institute
            var student = await _studentService.GetStudentByIdAsync(model.StudentId, instituteId.Value);
            if (student == null)
            {
                return BadRequest(new { success = false, message = "الطالب غير موجود في هذا المركز أو غير مصرح لك بتسجيل أوراد له." });
            }

            // Verify teacher has access to this student
            var teacherUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool hasAccess = student.Classes.Any(c => c.Teachers.Any(t => t.UserId == teacherUserId));
            if (!hasAccess && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return BadRequest(new { success = false, message = "غير مصرح لك بتسجيل أوراد لطالب خارج شعبك المخصصة." });
            }

            // Bind the wird to the teacher's currently selected class.
            if (Guid.TryParse(Request.Cookies["selectedClassId"], out var classId))
                model.ClassId = classId;
            else if (!model.ClassId.HasValue || model.ClassId == Guid.Empty)
            {
                var teacherClass = student.Classes.FirstOrDefault(c => c.Teachers.Any(t => t.UserId == teacherUserId));
                if (teacherClass != null)
                    model.ClassId = teacherClass.Id;
            }

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

            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return BadRequest(new { success = false, message = "تعذر تحديد المركز التابع له." });

            var student = await _studentService.GetStudentWithClassesAsync(studentId, instituteId.Value);
            if (student == null)
            {
                return NotFound(new { success = false, message = "تعذر العثور على بيانات الطالب في هذا المركز." });
            }

            var teacherUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool hasAccess = student.Classes.Any(c => c.Teachers.Any(t => t.UserId == teacherUserId));
            if (!hasAccess && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "غير مصرح لك بعرض بيانات طالب خارج شعبك المخصصة." });
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

        [HttpPost]
        public async Task<IActionResult> AssignMatn([FromBody] AssignMatnDto dto)
        {
            var teacherId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var tid) ? tid : Guid.Empty;
            if (teacherId == Guid.Empty)
                return BadRequest(new { success = false, message = "غير مصرح لك: تعذر تحديد هوية المعلم." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { success = false, message = string.IsNullOrEmpty(errors) ? "بيانات غير صالحة." : errors });
            }

            var (success, message, id) = await _matnAssignmentService.AssignAsync(dto, teacherId);
            return Json(new { success, message, id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

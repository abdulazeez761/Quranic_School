using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.Extensions;
using Hafiz.DTOs.Matn;
using Hafiz.DTOs.Wird;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class WirdController : Controller
    {
        private readonly IWirdService _wirdService;
        private readonly IClassService _classService;
        private readonly IMatnAssignmentService _matnAssignmentService;
        private readonly IMatnAssignmentRepository _matnAssignmentRepository;
        private readonly IStudentService _studentService;

        public WirdController(
            IWirdService wirdService,
            IClassService classService,
            IMatnAssignmentService matnAssignmentService,
            IMatnAssignmentRepository matnAssignmentRepository,
            IStudentService studentService)
        {
            _wirdService = wirdService;
            _classService = classService;
            _matnAssignmentService = matnAssignmentService;
            _matnAssignmentRepository = matnAssignmentRepository;
            _studentService = studentService;
        }

        private Guid GetTeacherId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim, out var id) ? id : Guid.Empty;
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

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? fromDate,
            string? toDate,
            string? tab = null,
            int page = 1,
            int pageSize = 12
        )
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            string? selectedClassFromCookies = Request.Cookies["selectedClassId"];
            Guid selectedClass;
            if (selectedClassFromCookies is not null && Guid.TryParse(selectedClassFromCookies, out var parsedClass))
                selectedClass = parsedClass;
            else
            {
                ModelState.AddModelError("NoClass", "you did not select a class");
                return View(new PagedResult<WirdAssignment>());
            }

            var classDto = await _classService.GetClassById(selectedClass, instituteId.Value);
            if (classDto == null)
            {
                ModelState.AddModelError("InvalidClass", "الشعبة المحددة غير موجودة في هذا المركز.");
                return View(new PagedResult<WirdAssignment>());
            }

            bool isMatnClass = (classDto.ProgramType == Hafiz.Domain.Enums.ProgramType.Matn);

            DateTime? parsedFromDate = null;
            DateTime? parsedToDate = null;
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var pFrom))
                parsedFromDate = pFrom;
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var pTo))
                parsedToDate = pTo;

            // 1. Fetch Quran Wird assignments
            List<WirdAssignment>? assignmentList =
                await _wirdService.GetWirdAssignmentsByClassIdAsync(
                    selectedClass,
                    fromDate,
                    toDate
                );
            var quranList = assignmentList ?? new List<WirdAssignment>();

            // 2. Fetch Matn assignments for the class and date range
            var matnList = (await _matnAssignmentService.GetByClassIdAsync(
                selectedClass,
                parsedFromDate,
                parsedToDate
            )).ToList();

            // 3. Prepare students list for the Matn / Wird drawer modal
            var studentsList = new List<SelectListItem>();
            var allClassStudents = new List<object>();
            if (classDto.StudentsIds != null && classDto.StudentsIds.Any())
            {
                foreach (var sId in classDto.StudentsIds)
                {
                    var s = await _studentService.GetByIdAsync(sId, instituteId.Value);
                    if (s != null)
                    {
                        var fn = s.FirstName ?? "";
                        var sn = s.SecondName ?? "";
                        var fullName = $"{fn} {sn}".Trim();
                        var initials = (
                            (fn.Length > 0 ? fn[0].ToString() : "")
                            + (sn.Length > 0 ? sn[0].ToString() : "")
                        ).ToUpper();

                        studentsList.Add(new SelectListItem
                        {
                            Value = sId.ToString(),
                            Text = fullName
                        });

                        allClassStudents.Add(new
                        {
                            id = sId.ToString(),
                            name = fullName,
                            initials = initials
                        });
                    }
                }
            }

            ViewBag.ClassDto = classDto;
            ViewBag.ClassName = classDto.Name;
            ViewBag.IsMatnClass = isMatnClass;
            ViewBag.StudyProgramName = classDto.StudyProgramName;
            ViewBag.Students = studentsList;
            ViewBag.AllClassStudents = allClassStudents;

            ViewBag.TotalQuranCount = quranList.Count;
            ViewBag.TotalMatnCount = matnList.Count;
            ViewBag.TotalCount = isMatnClass ? matnList.Count : (quranList.Count > 0 ? quranList.Count : matnList.Count);

            // Active Tab determination
            if (!string.IsNullOrEmpty(tab))
            {
                ViewBag.ActiveTab = tab.ToLower();
            }
            else
            {
                ViewBag.ActiveTab = isMatnClass ? "matn" : (quranList.Count == 0 && matnList.Count > 0 ? "matn" : "quran");
            }

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            // Pass paginated Matn assignments as well
            ViewBag.MatnAssignments = matnList.ToPagedResult(page, pageSize);

            var pagedQuran = quranList.ToPagedResult(page, pageSize);
            return View(pagedQuran);
        }

        [HttpGet]
        public async Task<IActionResult> GetWirdAssignmentById(Guid id)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            var assignment = await _wirdService.GetWirdAssignmentByIdAsync(id, instituteId.Value);
            if (assignment != null)
            {
                return Json(
                    new
                    {
                        assignment.Id,
                        assignment.Type,
                        assignment.Amount,
                        assignment.AmountUnit,
                        assignment.EquivalentPages,
                        assignment.FromJuz,
                        assignment.FromPage,
                        assignment.FromSurah,
                        assignment.FromAyah,
                        assignment.ToJuz,
                        assignment.ToPage,
                        assignment.ToSurah,
                        assignment.ToAyah,
                        assignment.Status,
                        assignment.IsUpcoming,
                        assignment.Note,
                    }
                );
            }

            var matn = await _matnAssignmentService.GetByIdAsync(id);
            if (matn != null)
            {
                return Json(
                    new
                    {
                        matn.Id,
                        Type = (int)matn.PerformanceType,
                        matn.PerformanceTypeName,
                        matn.Amount,
                        Unit = (int)matn.Unit,
                        matn.UnitName,
                        matn.ChapterName,
                        matn.FromNumber,
                        matn.ToNumber,
                        matn.Status,
                        matn.IsUpcoming,
                        matn.Note
                    }
                );
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest receivedData)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            AssignmentStatus status = (AssignmentStatus)
                Enum.Parse(typeof(AssignmentStatus), receivedData.Status, true);

            bool isWirdUpdated = await _wirdService.UpdateStatus(receivedData.Id, status, instituteId.Value);
            if (!isWirdUpdated)
            {
                var teacherId = GetTeacherId();
                var (matnSuccess, _) = await _matnAssignmentService.UpdateStatusAsync(
                    new UpdateMatnStatusDto
                    {
                        Id = receivedData.Id,
                        Status = status
                    },
                    teacherId
                );
                isWirdUpdated = matnSuccess;
            }

            return Json(new { success = isWirdUpdated });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWirdNote(
            [FromBody] UpdateNoteRequest updateNoteRequest
        )
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            bool isWirdUpdated = await _wirdService.UpdateWirdNote(
                updateNoteRequest.Id,
                updateNoteRequest.Note,
                instituteId.Value
            );

            if (!isWirdUpdated)
            {
                var matn = await _matnAssignmentRepository.GetByIdAsync(updateNoteRequest.Id);
                if (matn != null)
                {
                    matn.Note = updateNoteRequest.Note;
                    isWirdUpdated = await _matnAssignmentRepository.UpdateAsync(matn);
                }
            }

            return Json(new { success = isWirdUpdated });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            bool isWirdDeleted = await _wirdService.DeleteWirdAssignment(id, instituteId.Value);
            if (!isWirdDeleted)
            {
                var teacherId = GetTeacherId();
                var (matnSuccess, _) = await _matnAssignmentService.DeleteAsync(id, teacherId);
                isWirdDeleted = matnSuccess;
            }

            if (isWirdDeleted)
            {
                TempData["SuccessMessage"] = "تم حذف السجل بنجاح.";
                return Json(new { success = true, message = "تم الحذف بنجاح" });
            }
            else
            {
                TempData["ErrorMessage"] = "تعذر حذف السجل.";
                return Json(new { success = false, message = "حدث خطأ أثناء محاولة حذف السجل أو غير مصرح لك بذلك" });
            }
        }
    }
}

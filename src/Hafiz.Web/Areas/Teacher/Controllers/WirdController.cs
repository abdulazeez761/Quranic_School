using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.Extensions;
using Hafiz.DTOs.Wird;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class WirdController : Controller
    {
        private readonly IWirdService _wirdService;
        private readonly IClassService _classService;

        public WirdController(IWirdService wirdService, IClassService classService)
        {
            _wirdService = wirdService;
            _classService = classService;
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

            List<WirdAssignment>? assignmentList =
                await _wirdService.GetWirdAssignmentsByClassIdAsync(
                    selectedClass,
                    fromDate,
                    toDate
                );

            var list = assignmentList ?? new List<WirdAssignment>();
            ViewBag.TotalCount = list.Count;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            var paged = list.ToPagedResult(page, pageSize);

            return View(paged);
        }

        [HttpGet]
        public async Task<IActionResult> GetWirdAssignmentById(Guid id)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            var assignment = await _wirdService.GetWirdAssignmentByIdAsync(id, instituteId.Value);
            if (assignment == null)
                return NotFound();

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

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest receivedData)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            AssignmentStatus status = (AssignmentStatus)
                Enum.Parse(typeof(AssignmentStatus), receivedData.Status, true);

            bool isWIrdUpdated = await _wirdService.UpdateStatus(receivedData.Id, status, instituteId.Value);
            return Json(new { success = isWIrdUpdated });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWirdNote(
            [FromBody] UpdateNoteRequest updateNoteRequest
        )
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            bool isWIrdUpdated = await _wirdService.UpdateWirdNote(
                updateNoteRequest.Id,
                updateNoteRequest.Note,
                instituteId.Value
            );

            return Json(new { success = isWIrdUpdated });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var instituteId = GetInstituteId();
            if (!instituteId.HasValue)
                return Forbid();

            bool isWirdDeleted = await _wirdService.DeleteWirdAssignment(id, instituteId.Value);
            if (isWirdDeleted)
            {
                TempData["SuccessMessage"] = "Wird assignment deleted successfully.";
                return Json(new { success = true, message = "تم الحذف بنجاح" });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete wird assignment.";
                return Json(new { success = false, message = "حدث خطأ أثناء محاولة حذف السجل أو غير مصرح لك بذلك" });
            }
        }
    }
}

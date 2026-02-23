using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hafiz.Areas.Parent.Controllers
{
    [Authorize(Roles = "Parent")]
    [Area("Parent")]
    public class ChildrenController : Controller
    {
        private readonly ILogger<ChildrenController> _logger;
        private readonly IParentService _parentService;
        private readonly IStudentService _studentService;
        private readonly IParentNoteService _parentNoteService;

        public ChildrenController(
            ILogger<ChildrenController> logger,
            IParentService parentService,
            IStudentService studentService,
            IParentNoteService parentNoteService
        )
        {
            _logger = logger;
            _parentService = parentService;
            _studentService = studentService;
            _parentNoteService = parentNoteService;
        }

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var children = await _parentService.GetParentChildrenAsync(userId);

            // Collect attendance and wird stats for each child
            var childrenStats = new Dictionary<Guid, dynamic>();
            foreach (var child in children)
            {
                var attendance = await _studentService.GetStudentAttendanceAsync(child.UserId);
                var attendanceList = attendance.ToList();
                var wirds = await _studentService.GetStudentWirdsAsync(child.UserId);
                var wirdsList = wirds.ToList();

                int totalAtt = attendanceList.Count;
                int presentAtt = attendanceList.Count(a =>
                    a.Status == Hafiz.Models.AttendanceStatus.Present
                    || a.Status == Hafiz.Models.AttendanceStatus.Late
                );
                double attRate =
                    totalAtt > 0 ? Math.Round((double)presentAtt / totalAtt * 100, 0) : 0;

                childrenStats[child.UserId] = new
                {
                    AttendanceRate = attRate,
                    TotalWirds = wirdsList.Count,
                    CompletedWirds = wirdsList.Count(w => w.IsCompleted),
                    MemorizedJuz = child.MemorizedJuz,
                    HasActiveMeeting = child.Classes?.Any(c => c.IsMeetingActive) ?? false,
                };
            }

            ViewBag.ChildrenStats = childrenStats;

            return View(children);
        }

        public async Task<IActionResult> Details(Guid studentId, int page = 1)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify that this student belongs to this parent
            var children = await _parentService.GetParentChildrenAsync(userId);
            var child = children.FirstOrDefault(c => c.UserId == studentId);

            if (child == null)
            {
                return NotFound("Student not found or does not belong to this parent.");
            }

            // Get detailed information about the student
            var attendance = await _studentService.GetStudentAttendanceAsync(studentId);
            var attendanceList = attendance.ToList();
            var paginatedWirds = await _studentService.GetStudentWirdsPaginatedAsync(
                studentId,
                page,
                5
            );
            var notes = await _parentNoteService.GetNotesByStudentIdAsync(studentId);

            // Attendance statistics
            int totalAtt = attendanceList.Count;
            int presentCount = attendanceList.Count(a =>
                a.Status == Hafiz.Models.AttendanceStatus.Present
            );
            int lateCount = attendanceList.Count(a =>
                a.Status == Hafiz.Models.AttendanceStatus.Late
            );
            int absentCount = attendanceList.Count(a =>
                a.Status == Hafiz.Models.AttendanceStatus.Absent
            );
            int excusedCount = attendanceList.Count(a =>
                a.Status == Hafiz.Models.AttendanceStatus.Excused
            );
            double attRate =
                totalAtt > 0
                    ? Math.Round((double)(presentCount + lateCount) / totalAtt * 100, 1)
                    : 0;

            ViewBag.Attendance = attendanceList;
            ViewBag.PaginatedWirds = paginatedWirds;
            ViewBag.ParentNotes = notes;
            ViewBag.CurrentPage = page;
            ViewBag.AttendanceRate = attRate;
            ViewBag.PresentCount = presentCount;
            ViewBag.LateCount = lateCount;
            ViewBag.AbsentCount = absentCount;
            ViewBag.ExcusedCount = excusedCount;

            return View(child);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(Guid studentId, string content)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify parent owns this student
            var children = await _parentService.GetParentChildrenAsync(userId);
            var child = children.FirstOrDefault(c => c.UserId == studentId);

            if (child == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "NoteContentRequired";
                return RedirectToAction(nameof(Details), new { studentId });
            }

            try
            {
                await _parentNoteService.CreateNoteAsync(studentId, content, userId);
                TempData["Success"] = "NoteCreated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parent note");
                TempData["Error"] = "NoteCreateError";
            }

            return RedirectToAction(nameof(Details), new { studentId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNote(Guid noteId, Guid studentId)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var canManage = await _parentNoteService.CanUserManageNoteAsync(userId, noteId);
                if (!canManage)
                {
                    TempData["Error"] = "NoteDeleteNotAllowed";
                    return RedirectToAction(nameof(Details), new { studentId });
                }

                await _parentNoteService.DeleteNoteAsync(noteId, userId);
                TempData["Success"] = "NoteDeleted";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parent note");
                TempData["Error"] = "NoteDeleteError";
            }

            return RedirectToAction(nameof(Details), new { studentId });
        }
    }
}

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hifz.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "Teacher,Student,Admin")]
    public class MeetingController : Controller
    {
        private readonly IMeetingService _meetingService;
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(IMeetingService meetingService, ILogger<MeetingController> logger)
        {
            _meetingService = meetingService;
            _logger = logger;
        }

        public async Task<IActionResult> Room(Guid classId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized();
                }

                var userRole = GetUserRole();

                // Check if user can join the meeting
                var (canJoin, reason, displayName, role) = await _meetingService
                    .CanJoinMeetingAsync(userId, classId, userRole);

                if (!canJoin)
                {
                    TempData["Error"] = reason;
                    return RedirectToMeetingList();
                }

                // Get class details
                var cls = await _meetingService.GetClassWithDetailsAsync(classId);
                if (cls == null)
                {
                    TempData["Error"] = "Class not found";
                    return RedirectToMeetingList();
                }

                // Prepare view data
                var roomName = BuildRoomName(classId);

                ViewBag.RoomName = roomName;
                ViewBag.DisplayName = displayName;
                ViewBag.UserRole = role;
                ViewBag.ClassName = cls.Name;
                ViewBag.ClassId = classId;
                ViewBag.IsTeacher = User.IsInRole("Teacher");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining meeting for class {ClassId}", classId);
                TempData["Error"] = "An error occurred while joining the meeting.";
                return RedirectToMeetingList();
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }

        private string GetUserRole()
        {
            if (User.IsInRole("Teacher"))
                return "Teacher";
            if (User.IsInRole("Student"))
                return "Student";
            if (User.IsInRole("Admin"))
                return "Admin";
            return string.Empty;
        }

        private string BuildRoomName(Guid classId)
        {
            // Deterministic: same class → same room
            return $"HifzClass_{classId:N}";
        }

        private IActionResult RedirectToMeetingList()
        {
            if (User.IsInRole("Teacher"))
                return RedirectToAction(
                    "ManageMeetings",
                    "TeacherClasses",
                    new { area = "Teacher" }
                );
            else if (User.IsInRole("Student"))
                return RedirectToAction("Index", "Meeting", new { area = "Student" });
            else
                return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion
    }
}

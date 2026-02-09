using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hifz.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class MeetingController : Controller
    {
        private readonly IMeetingService _meetingService;
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(IMeetingService meetingService, ILogger<MeetingController> logger)
        {
            _meetingService = meetingService;
            _logger = logger;
        }

        /// <summary>
        /// Display list of active meetings for student's enrolled classes
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var studentId = GetCurrentUserId();
                if (studentId == Guid.Empty)
                {
                    return Unauthorized();
                }

                var activeMeetings = await _meetingService.GetStudentActiveMeetingsAsync(
                    studentId
                );

                return View(activeMeetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active meetings for student");
                TempData["Error"] = "An error occurred while loading meetings.";
                return View(Enumerable.Empty<Hifz.Models.Class>());
            }
        }

        /// <summary>
        /// Join a specific meeting
        /// </summary>
        public async Task<IActionResult> Join(Guid classId)
        {
            try
            {
                _logger.LogInformation("Student attempting to join meeting for class {ClassId}", classId);
                
                var studentId = GetCurrentUserId();
                if (studentId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthorized access attempt - no user ID");
                    return Unauthorized();
                }

                _logger.LogInformation("Student {StudentId} joining class {ClassId}", studentId, classId);

                // Check if student can join
                var (canJoin, reason, displayName, role) = await _meetingService
                    .CanJoinMeetingAsync(studentId, classId, "Student");

                if (!canJoin)
                {
                    _logger.LogWarning("Student {StudentId} cannot join class {ClassId}: {Reason}", 
                        studentId, classId, reason);
                    TempData["Error"] = reason;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Student {StudentId} authorized to join, redirecting to room", studentId);

                // Redirect to student meeting room (uses student layout)
                return RedirectToAction(
                    "Room",
                    "Meeting",
                    new { area = "Student", classId = classId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining meeting for class {ClassId}", classId);
                TempData["Error"] = "An error occurred while joining the meeting.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Meeting room view for students
        /// </summary>
        public async Task<IActionResult> Room(Guid classId)
        {
            try
            {
                var studentId = GetCurrentUserId();
                if (studentId == Guid.Empty)
                {
                    return Unauthorized();
                }

                // Check if student can join
                var (canJoin, reason, displayName, role) = await _meetingService
                    .CanJoinMeetingAsync(studentId, classId, "Student");

                if (!canJoin)
                {
                    TempData["Error"] = reason;
                    return RedirectToAction(nameof(Index));
                }

                // Get class details
                var cls = await _meetingService.GetClassWithDetailsAsync(classId);
                if (cls == null)
                {
                    TempData["Error"] = "Class not found";
                    return RedirectToAction(nameof(Index));
                }

                // Prepare view data
                var roomName = BuildRoomName(classId);

                ViewBag.RoomName = roomName;
                ViewBag.DisplayName = displayName;
                ViewBag.UserRole = role;
                ViewBag.ClassName = cls.Name;
                ViewBag.ClassId = classId;
                ViewBag.IsTeacher = false; // Students are not teachers

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading meeting room for class {ClassId}", classId);
                TempData["Error"] = "An error occurred while loading the meeting room.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }

        private string BuildRoomName(Guid classId)
        {
            // Same room name format as teacher - ensures they join the same room
            return $"HifzClass_{classId:N}";
        }

        #endregion
    }
}

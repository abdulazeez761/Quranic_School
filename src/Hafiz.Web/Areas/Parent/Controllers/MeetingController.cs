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
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
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
        /// Display list of active meetings for all children's classes
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var parentId = GetCurrentUserId();
                if (parentId == Guid.Empty)
                {
                    return Unauthorized();
                }

                var activeMeetings = await _meetingService.GetParentActiveMeetingsAsync(parentId);

                return View(activeMeetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active meetings for parent");
                TempData["Error"] = "An error occurred while loading meetings.";
                return View(Enumerable.Empty<Hafiz.Models.Class>());
            }
        }

        /// <summary>
        /// Join a specific meeting for a child's class
        /// </summary>
        public async Task<IActionResult> Join(Guid classId, Guid studentId)
        {
            try
            {
                _logger.LogInformation("Parent attempting to join meeting for class {ClassId}, student {StudentId}", classId, studentId);
                
                var parentId = GetCurrentUserId();
                if (parentId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthorized access attempt - no user ID");
                    return Unauthorized();
                }

                // Verify parent owns this student
                var isParentOfStudent = await _meetingService.IsParentOfStudentAsync(parentId, studentId);
                if (!isParentOfStudent)
                {
                    _logger.LogWarning("Parent {ParentId} attempted to join meeting for student {StudentId} they don't own", 
                        parentId, studentId);
                    TempData["Error"] = "You can only join meetings for your own children.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if parent can join (meeting is active and student is in class)
                var (canJoin, reason, displayName, role) = await _meetingService
                    .CanJoinMeetingAsync(studentId, classId, "Parent");

                if (!canJoin)
                {
                    _logger.LogWarning("Parent {ParentId} cannot join class {ClassId}: {Reason}", 
                        parentId, classId, reason);
                    TempData["Error"] = reason;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Parent {ParentId} authorized to join, redirecting to room", parentId);

                // Redirect to parent meeting room
                return RedirectToAction(
                    "Room",
                    "Meeting",
                    new { area = "Parent", classId = classId, studentId = studentId }
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
        /// Meeting room view for parents
        /// </summary>
        public async Task<IActionResult> Room(Guid classId, Guid studentId)
        {
            try
            {
                var parentId = GetCurrentUserId();
                if (parentId == Guid.Empty)
                {
                    return Unauthorized();
                }

                // Verify parent owns this student
                var isParentOfStudent = await _meetingService.IsParentOfStudentAsync(parentId, studentId);
                if (!isParentOfStudent)
                {
                    TempData["Error"] = "You can only join meetings for your own children.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if parent can join
                var (canJoin, reason, displayName, role) = await _meetingService
                    .CanJoinMeetingAsync(studentId, classId, "Parent");

                if (!canJoin)
                {
                    TempData["Error"] = reason;
                    return RedirectToAction(nameof(Index));
                }

                // Get class and student details
                var cls = await _meetingService.GetClassWithDetailsAsync(classId);
                if (cls == null)
                {
                    TempData["Error"] = "Class not found";
                    return RedirectToAction(nameof(Index));
                }

                var student = await _meetingService.GetStudentDetailsAsync(studentId);
                if (student == null)
                {
                    TempData["Error"] = "Student not found";
                    return RedirectToAction(nameof(Index));
                }

                // Prepare view data
                var roomName = BuildRoomName(classId);

                ViewBag.RoomName = roomName;
                ViewBag.DisplayName = $"Parent of {student.StudentInfo.FirstName}";
                ViewBag.UserRole = "Parent";
                ViewBag.ClassName = cls.Name;
                ViewBag.ClassId = classId;
                ViewBag.StudentId = studentId;
                ViewBag.IsTeacher = false;

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
            // Same room name format as teacher/student - ensures they join the same room
            return $"HafizClass_{classId:N}";
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "Teacher")]
    public class TeacherClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMeetingService _meetingService;
        private readonly ILogger<TeacherClassesController> _logger;

        public TeacherClassesController(
            ILogger<TeacherClassesController> logger,
            ApplicationDbContext context,
            IMeetingService meetingService
        )
        {
            _logger = logger;
            _context = context;
            _meetingService = meetingService;
        }

        public async Task<IActionResult> ManageMeetings()
        {
            try
            {
                var teacherId = GetCurrentUserId();
                if (teacherId == Guid.Empty)
                {
                    return Unauthorized();
                }

                var teacher = await _context
                    .Teachers.Include(t => t.Classes)
                    .ThenInclude(c => c.Students)
                    .FirstOrDefaultAsync(t => t.UserId == teacherId);

                if (teacher == null)
                {
                    return NotFound("Teacher profile not found");
                }

                return View(teacher.Classes ?? new List<Class>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading classes for teacher");
                TempData["Error"] = "An error occurred while loading your classes.";
                return View(new List<Class>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMeeting(Guid classId)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                if (teacherId == Guid.Empty)
                    return Unauthorized();

                var (success, message) = await _meetingService.StartMeetingAsync(
                    classId,
                    teacherId
                );

                if (!success)
                {
                    TempData["Error"] = message;
                    return RedirectToAction(nameof(ManageMeetings));
                }

                TempData["Success"] = message;
                return RedirectToAction(
                    "Room",
                    "Meeting",
                    new { area = "Teacher", classId = classId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting meeting for class {ClassId}", classId);
                TempData["Error"] = "An error occurred while starting the meeting.";
                return RedirectToAction(nameof(ManageMeetings));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndMeeting(Guid classId)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                if (teacherId == Guid.Empty)
                    return Unauthorized();

                var (success, message) = await _meetingService.EndMeetingAsync(classId, teacherId);

                if (!success)
                {
                    TempData["Warning"] = message;
                }
                else
                {
                    TempData["Success"] = message;
                }

                return RedirectToAction(nameof(ManageMeetings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending meeting for class {ClassId}", classId);
                TempData["Error"] = "An error occurred while ending the meeting.";
                return RedirectToAction(nameof(ManageMeetings));
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}

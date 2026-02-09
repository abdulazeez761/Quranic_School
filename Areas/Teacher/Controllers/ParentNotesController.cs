using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hifz.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class ParentNotesController : Controller
    {
        private readonly ILogger<ParentNotesController> _logger;
        private readonly IParentNoteService _parentNoteService;
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;

        public ParentNotesController(
            ILogger<ParentNotesController> logger,
            IParentNoteService parentNoteService,
            IStudentService studentService,
            IClassService classService
        )
        {
            _logger = logger;
            _parentNoteService = parentNoteService;
            _studentService = studentService;
            _classService = classService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid studentId, string content)
        {
            try
            {
                _logger.LogInformation($"Attempting to create note for student {studentId} with content length {content?.Length}");

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Note content is empty");
                    TempData["Error"] = "Note content cannot be empty.";
                    return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogError("User ID claim is missing");
                    TempData["Error"] = "User not identified.";
                    return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
                }

                Guid userId = Guid.Parse(userIdClaim);
                _logger.LogInformation($"User ID: {userId}");
                
                await _parentNoteService.CreateNoteAsync(studentId, content, userId);
                
                _logger.LogInformation("Note created successfully");
                TempData["Success"] = "Note added successfully.";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating parent note for student {studentId}. Message: {ex.Message}");
                TempData["Error"] = $"Failed to create note: {ex.Message}";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid noteId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["Error"] = "Note content cannot be empty.";
                    return RedirectToAction("Index", "Student", new { area = "Teacher" });
                }

                Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var note = await _parentNoteService.UpdateNoteAsync(noteId, content, userId);
                
                TempData["Success"] = "Note updated successfully.";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = note.StudentId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You don't have permission to update this note.";
                return RedirectToAction("Index", "Student", new { area = "Teacher" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parent note");
                TempData["Error"] = "Failed to update note.";
                return RedirectToAction("Index", "Student", new { area = "Teacher" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid noteId, Guid studentId)
        {
            try
            {
                Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                await _parentNoteService.DeleteNoteAsync(noteId, userId);
                
                TempData["Success"] = "Note deleted successfully.";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You don't have permission to delete this note.";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parent note");
                TempData["Error"] = "Failed to delete note.";
                return RedirectToAction("Details", "Student", new { area = "Teacher", id = studentId });
            }
        }
    }
}

using System.Security.Claims;
using System.Threading.Tasks;
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

        public WirdController(IWirdService wirdService)
        {
            _wirdService = wirdService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? fromDate, string? toDate)
        {
            string? selectedClassFromCookies = Request.Cookies["selectedClassId"];
            Guid selectedClass;
            if (selectedClassFromCookies is not null)
                selectedClass = Guid.Parse(selectedClassFromCookies);
            else
            {
                ModelState.AddModelError("NoClass", "you did not select a class");
                return View(new List<WirdAssignment>());
            }

            List<WirdAssignment>? assignmentList =
                await _wirdService.GetWirdAssignmentsByClassIdAsync(
                    selectedClass,
                    fromDate,
                    toDate
                );

            return View(assignmentList);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest receivedData)
        {
            AssignmentStatus status = (AssignmentStatus) // this because the enum.parse returns an object so we tell the variable I will return the AssignmentStatus so it matches the declaration
                Enum.Parse(typeof(AssignmentStatus), receivedData.Status, true);

            bool isWIrdUpdated = await _wirdService.UpdateStatus(receivedData.Id, status);
            return Json(new { success = isWIrdUpdated });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWirdNote(
            [FromBody] UpdateNoteRequest updateNoteRequest
        )
        {
            bool isWIrdUpdated = await _wirdService.UpdateWirdNote(
                updateNoteRequest.Id,
                updateNoteRequest.Note
            );

            return Json(new { success = isWIrdUpdated });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            bool isWirdDeleted = await _wirdService.DeleteWirdAssignment(id);
            if (isWirdDeleted)
            {
                TempData["SuccessMessage"] = "Wird assignment deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete wird assignment.";
            }

            return RedirectToAction("Index");
        }
    }
}

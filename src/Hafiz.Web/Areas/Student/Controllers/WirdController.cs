using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Student.Controllers
{
    [Authorize(Roles = "Student")]
    [Area("Student")]
    public class WirdController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IMatnAssignmentService _matnAssignmentService;

        public WirdController(IStudentService studentService, IMatnAssignmentService matnAssignmentService)
        {
            _studentService = studentService;
            _matnAssignmentService = matnAssignmentService;
        }

        public async Task<IActionResult> Index(
            string? status,
            string? type,
            string? tab = null,
            int page = 1,
            int pageSize = 10
        )
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            StudentModel? student = await _studentService.GetStudentByUserIdAsync(userId);

            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            bool? isCompleted = status?.ToLower() switch
            {
                "completed" => true,
                "pending" => false,
                _ => null,
            };

            bool? isUpcoming = status?.ToLower() == "upcoming" ? true : null;

            AssignmentType? assignmentType = null;
            if (
                !string.IsNullOrEmpty(type)
                && Enum.TryParse(type, true, out AssignmentType parsedType)
            )
            {
                assignmentType = parsedType;
            }

            var paginatedWirds = await _studentService.GetStudentWirdsPaginatedAsync(
                student.UserId,
                page,
                pageSize,
                isCompleted,
                assignmentType,
                isUpcoming
            );

            var matnAssignments = (await _matnAssignmentService.GetByStudentIdAsync(student.UserId)).ToList();

            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.Student = student;
            ViewBag.PageSize = pageSize;
            ViewBag.MatnAssignments = matnAssignments;
            ViewBag.TotalMatnAssignments = matnAssignments.Count;
            ViewBag.ActiveTab = !string.IsNullOrEmpty(tab) ? tab.ToLower() : (paginatedWirds.TotalCount == 0 && matnAssignments.Count > 0 ? "matn" : "quran");

            return View(paginatedWirds);
        }
    }
}

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Models;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hifz.Models.Student;

namespace Hifz.Areas.Student.Controllers
{
    [Authorize(Roles = "Student")]
    [Area("Student")]
    public class WirdController : Controller
    {
        private readonly IStudentService _studentService;

        public WirdController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index(
            string? status,
            string? type,
            int page = 1,
            int pageSize = 5
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
                assignmentType
            );

            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.Student = student;
            ViewBag.PageSize = pageSize;

            return View(paginatedWirds);
        }
    }
}

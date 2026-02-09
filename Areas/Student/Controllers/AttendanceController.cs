using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hifz.Models.Student;

namespace Hifz.Areas.Student.Controllers
{
    [Authorize(Roles = "Student")]
    [Area("Student")]
    public class AttendanceController : Controller
    {
        private readonly IStudentService _studentService;

        public AttendanceController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            StudentModel? student = await _studentService.GetStudentByUserIdAsync(userId);
            
            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            var attendance = await _studentService.GetStudentAttendanceAsync(student.UserId);

            // Filter by date range if provided
            if (fromDate.HasValue)
            {
                attendance = attendance.Where(a => a.Date >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                attendance = attendance.Where(a => a.Date <= toDate.Value);
            }

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Student = student;

            return View(attendance.OrderByDescending(a => a.Date));
        }
    }
}

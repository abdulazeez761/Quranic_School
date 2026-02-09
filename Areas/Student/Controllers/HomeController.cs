using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentModel = Hifz.Models.Student;

namespace Hifz.Areas.Student.Controllers
{
    [Authorize(Roles = "Student")]
    [Area("Student")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStudentService _studentService;

        public HomeController(ILogger<HomeController> logger, IStudentService studentService)
        {
            _logger = logger;
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            StudentModel? student = await _studentService.GetStudentByUserIdAsync(userId);

            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            // Get recent attendance and wird assignments for dashboard
            var attendance = await _studentService.GetStudentAttendanceAsync(student.UserId);
            var wirds = await _studentService.GetStudentWirdsAsync(student.UserId);

            ViewBag.Student = student;
            ViewBag.RecentAttendance = attendance.OrderByDescending(a => a.Date).Take(5);
            ViewBag.PendingWirds = wirds
                .Where(w => !w.IsCompleted)
                .OrderBy(w => w.AssignedDate)
                .Take(5);

            return View(student);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

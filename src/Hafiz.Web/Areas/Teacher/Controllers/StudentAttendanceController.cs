using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Attendance;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "AdminOrTeacher")]
    public class StudentAttendanceController : Controller
    {
        private readonly ILogger<StudentAttendanceController> _logger;
        private readonly IClassService _classService;
        private readonly IStudentAttendanceService _studentAttendanceService;

        public StudentAttendanceController(
            ILogger<StudentAttendanceController> logger,
            IClassService classService,
            IStudentAttendanceService studentAttendanceService
        )
        {
            _logger = logger;
            _classService = classService;
            _studentAttendanceService = studentAttendanceService;
        }

        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewData["Role"] = role;
            if (User.FindFirstValue(ClaimTypes.Role) == UserRole.Teacher.ToString())
                await PopulateClassesDropdown(Guid.Parse(id!));
            else
                await PopulateClassesDropdown(Guid.Empty);

            var students = await _studentAttendanceService.GetStudentsByClass(
                Guid.Parse(ViewBag.Classes[0].Value),
                DateTime.Now.Date
            );

            return View(students);
        }

        [HttpPost]
        public async Task SaveAttendance([FromBody] SaveStudentAttendanceDto saveAttendance)
        {
            await _studentAttendanceService.AttendStudent(saveAttendance);
        }

        public async Task<IActionResult> GetStudentsByClassId(Guid classId, DateTime date)
        {
            var result = await _studentAttendanceService.GetStudentsByClass(classId, date);

            return Json(result);
        }

        private async Task PopulateClassesDropdown(Guid id)
        {
            var classes = await _classService.GetClassesAsync();
            if (id != Guid.Empty)
                classes = classes.Where(c => c.TeacherIds.Contains(id)).ToList();

            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
        }
    }
}

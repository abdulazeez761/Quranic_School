using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.Extensions;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Student.Controllers
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

        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 15
        )
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

            var list = attendance.OrderByDescending(a => a.Date).ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Student = student;
            ViewBag.TotalPresent = list.Count(a => a.Status == Hafiz.Models.AttendanceStatus.Present);
            ViewBag.TotalAbsent = list.Count(a => a.Status == Hafiz.Models.AttendanceStatus.Absent);
            ViewBag.TotalLate = list.Count(a => a.Status == Hafiz.Models.AttendanceStatus.Late);
            ViewBag.TotalExcused = list.Count(a => a.Status == Hafiz.Models.AttendanceStatus.Excused);

            var pagedAttendance = list.ToPagedResult(page, pageSize);

            return View(pagedAttendance);
        }
    }
}

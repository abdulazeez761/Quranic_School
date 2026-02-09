using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hifz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeacherAttendanceController : Controller
    {
        // private readonly IAttendanceService _attendanceService;
        private readonly ITeacherService _teacherService;
        private readonly IClassService _classService;
        private readonly ITeacherAttendanceService _teacherAttendanceService;

        public TeacherAttendanceController(
            ITeacherService teacherService,
            IClassService classService,
            ITeacherAttendanceService teacherAttendance
        )
        {
            _teacherService = teacherService;
            _classService = classService;
            _teacherAttendanceService = teacherAttendance;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await PopulateClassesDropdown();

            var teachers = await _teacherAttendanceService.GetTeachersByClass(
                Guid.Parse(ViewBag.Classes[0].Value),
                DateTime.Now.Date
            );

            return View(teachers);
        }

        [HttpPost]
        public async Task SaveAttendance([FromBody] SaveTeacherAttendanceDto saveAttendance)
        {
            await _teacherAttendanceService.AttendTeacher(saveAttendance);
        }

        // [Area("Admin")]

        public async Task<IActionResult> GetTeachersByClass(Guid classId, DateTime date)
        {
            var result = await _teacherAttendanceService.GetTeachersByClass(classId, date);

            return Json(result);
        }

        private async Task PopulateClassesDropdown()
        {
            var classes = await _classService.GetClassesAsync();
            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
        }
    }
}

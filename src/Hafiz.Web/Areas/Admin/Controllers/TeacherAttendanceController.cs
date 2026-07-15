using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Hafiz.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hafiz.Areas.Admin.Controllers
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

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirst("InstituteId");
            return claim != null ? Guid.Parse(claim.Value) : null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await PopulateClassesDropdown();
            Guid? instituteId = GetInstituteId();
            IEnumerable<TeacherAttendanceDto> teachers;
            if (ViewBag.Classes[0].Value != "all")
                teachers = await _teacherAttendanceService.GetTeachersByClass(
                    Guid.Parse(ViewBag.Classes[0].Value),
                    TimeZoneHelper.GetUserToday(HttpContext)
                );
            else
                teachers = await _teacherAttendanceService.GetAllTeachersByDateAndInstitute(
                    TimeZoneHelper.GetUserToday(HttpContext),
                    instituteId.Value
                );

            teachers = teachers.Where(t => t.InstituteId == instituteId);
            return View(teachers);
        }

        [HttpPost]
        public async Task SaveAttendance([FromBody] SaveTeacherAttendanceDto saveAttendance)
        {
            if (saveAttendance.ClassID == Guid.Empty)
                await _teacherAttendanceService.AttendTeacherToAllClasses(saveAttendance);
            else
                await _teacherAttendanceService.AttendTeacher(saveAttendance);
        }

        // [Area("Admin")]

        public async Task<IActionResult> GetTeachersByClass(Guid classId, DateTime date)
        {
            IEnumerable<TeacherAttendanceDto> result;
            if (classId == Guid.Empty)
            {
                Guid? instituteId = GetInstituteId();
                result = await _teacherAttendanceService.GetAllTeachersByDateAndInstitute(
                    date,
                    instituteId.Value
                );
                return Json(result);
            }
            result = await _teacherAttendanceService.GetTeachersByClass(classId, date);

            return Json(result);
        }

        private async Task PopulateClassesDropdown()
        {
            List<ClassDto>? classes = await _classService.GetClassesAsync();
            classes = classes.Where(c => c.InstituteId == GetInstituteId()).ToList();
            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
            ViewBag.Classes.Insert(0, new SelectListItem { Value = "all", Text = "Select Class" });
        }
    }
}

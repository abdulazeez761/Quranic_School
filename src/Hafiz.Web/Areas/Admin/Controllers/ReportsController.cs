using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Reports;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;

        public ReportsController(IStudentService studentService, IClassService classService)
        {
            _studentService = studentService;
            _classService = classService;
        }

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        // التقارير اليومية: حضور وغياب وأوراد الطلاب مفصّلة حسب كل شعبة
        [HttpGet]
        public async Task<IActionResult> Daily(DateTime? date = null)
        {
            var selectedDate = (date ?? DateTime.Today).Date;
            var instituteId = GetInstituteId();

            IEnumerable<StudentModel> students;
            IEnumerable<Class> classes;

            if (instituteId.HasValue)
            {
                students = await _studentService.GetAllByInstituteAsync(instituteId.Value);
                classes = await _classService.ViewClassesByInstitute(instituteId.Value);
            }
            else
            {
                // students = await _studentService.GetAllAsync();
                // classes = await _classService.ViewClasses();
                return Forbid();
            }

            var studentList = students.ToList();

            var classReports = classes
                .OrderBy(c => c.Name)
                .Select(c => new ClassDailyReport
                {
                    ClassId = c.Id,
                    ClassName = c.Name,
                    Students = studentList
                        .Where(s => s.Classes.Any(sc => sc.Id == c.Id))
                        .OrderBy(s => s.StudentInfo.FirstName)
                        .Select(s => new StudentDailyRow
                        {
                            StudentId = s.UserId,
                            FullName = $"{s.StudentInfo.FirstName} {s.StudentInfo.SecondName}",
                            Status = s
                                .Attendances.Where(a =>
                                    a.ClassId == c.Id && a.Date.Date == selectedDate
                                )
                                .Select(a => (AttendanceStatus?)a.Status)
                                .FirstOrDefault(),
                            Wirds = s
                                .wirds.Where(w => w.AssignedDate.Date == selectedDate)
                                .OrderBy(w => w.Type)
                                .ToList(),
                        })
                        .ToList(),
                })
                .ToList();

            var viewModel = new AdminDailyReportsViewModel
            {
                Date = selectedDate,
                Classes = classReports,
            };

            return View(viewModel);
        }
    }
}

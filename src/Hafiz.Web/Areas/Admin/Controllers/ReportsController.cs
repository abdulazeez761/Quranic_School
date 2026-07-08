using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Reports;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Hafiz.Web.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly IWirdService _wirdService;
        private readonly IInstituteService _instituteService;

        public ReportsController(
            IStudentService studentService,
            IClassService classService,
            IWirdService wirdService,
            IInstituteService instituteService
        )
        {
            _studentService = studentService;
            _classService = classService;
            _wirdService = wirdService;
            _instituteService = instituteService;
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
            DateTime selectedDate = (date ?? DateTime.Today).Date;
            var instituteId = GetInstituteId();

            IEnumerable<StudentModel> students;
            IEnumerable<Class> classes;

            if (instituteId.HasValue)
            {
                classes = await _classService.ViewClassesByInstituteAndClassDaysAsync(
                    instituteId.Value,
                    selectedDate
                );

                students = await _studentService.GetStudentByInstituteIdAsyncAndClassDay(
                    instituteId.Value,
                    selectedDate
                );
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
                            // Only wirds assigned within THIS class. Legacy wirds with
                            // ClassId == null fall back to student→class membership so
                            // pre-scoping history still shows up somewhere.
                            Wirds = s
                                .wirds.Where(w =>
                                    w.AssignedDate.Date == selectedDate
                                    && (
                                        w.ClassId == c.Id
                                        || (w.ClassId == null && s.Classes.Any(sc => sc.Id == c.Id))
                                    )
                                )
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

        // تقرير الأوراد: تصفية متقدّمة + إحصائيات + ترتيب الطلاب + تصدير
        [HttpGet]
        public async Task<IActionResult> Wirds(WirdReportFilterDto filter)
        {
            var isSuperAdmin = ResolveScope(filter);
            if (isSuperAdmin is null)
                return Forbid();

            NormalizePaging(filter);

            var vm = await _wirdService.GetWirdReportAsync(filter);
            vm.IsCrossCenter = isSuperAdmin.Value;

            await PopulateFilterOptionsAsync(vm, filter, isSuperAdmin.Value);

            return View(vm);
        }

        // تصدير التقرير كملف Excel (بدون ترقيم صفحات: كل الأسطر المطابقة للتصفية)
        [HttpGet]
        public async Task<IActionResult> ExportWirdsExcel(WirdReportFilterDto filter)
        {
            if (ResolveScope(filter) is null)
                return Forbid();

            var vm = await _wirdService.GetWirdReportForExportAsync(filter);

            var bytes = WirdReportExcelExporter.Build(vm);
            var fileName = $"wird-report-{DateTime.Today:yyyy-MM-dd}.xlsx";
            return File(bytes, WirdReportExcelExporter.ContentType, fileName);
        }

        // يحدّد نطاق المركز ويُعيد هل المستخدم مشرف نظام.
        // للمشرف: يُفرض مركزه على التصفية. لمشرف النظام: يُترك InstituteId من الاستعلام (null = الكل).
        // يُعيد null عندما يتعذّر تحديد نطاق صالح (مشرف بلا مركز) للدلالة على المنع.
        private bool? ResolveScope(WirdReportFilterDto filter)
        {
            if (User.IsInRole("SuperAdmin"))
                return true;

            var instituteId = GetInstituteId();
            if (instituteId is null)
                return null;

            filter.InstituteId = instituteId;
            return false;
        }

        private static void NormalizePaging(WirdReportFilterDto filter)
        {
            if (filter.Page < 1)
                filter.Page = 1;
            if (filter.PageSize < 1)
                filter.PageSize = 25;
        }

        private async Task PopulateFilterOptionsAsync(
            WirdReportViewModel vm,
            WirdReportFilterDto filter,
            bool isSuperAdmin
        )
        {
            IEnumerable<Class> classes;
            IEnumerable<StudentModel> students;

            if (filter.InstituteId.HasValue)
            {
                classes = await _classService.ViewClassesByInstitute(filter.InstituteId.Value);
                students = await _studentService.GetAllByInstituteAsync(filter.InstituteId.Value);
            }
            else
            {
                classes = await _classService.ViewClasses();
                students = await _studentService.GetAllAsync();
            }

            vm.ClassOptions = classes
                .OrderBy(c => c.Name)
                .Select(c => new SelectOption(c.Id.ToString(), c.Name))
                .ToList();

            vm.StudentOptions = students
                .OrderBy(s => s.StudentInfo.FirstName)
                .Select(s => new SelectOption(
                    s.UserId.ToString(),
                    $"{s.StudentInfo.FirstName} {s.StudentInfo.SecondName}"
                ))
                .ToList();

            if (isSuperAdmin)
            {
                var institutes = await _instituteService.GetAllAsync();
                vm.InstituteOptions = institutes
                    .OrderBy(i => i.Name)
                    .Select(i => new SelectOption(i.Id.ToString(), i.Name))
                    .ToList();
            }
        }
    }
}

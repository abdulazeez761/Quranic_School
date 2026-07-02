using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs.Reports;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Hafiz.Web.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly IWirdService _wirdService;
        private readonly IInstituteService _instituteService;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;

        public ReportsController(
            IWirdService wirdService,
            IInstituteService instituteService,
            IClassService classService,
            IStudentService studentService
        )
        {
            _wirdService = wirdService;
            _instituteService = instituteService;
            _classService = classService;
            _studentService = studentService;
        }

        // تقرير الأوراد على مستوى النظام: عبر كل المراكز أو مركز مختار، مع نفس التصفية والإحصائيات.
        [HttpGet]
        public async Task<IActionResult> Wirds(WirdReportFilterDto filter)
        {
            NormalizePaging(filter);

            var vm = await _wirdService.GetWirdReportAsync(filter);
            vm.IsCrossCenter = true;

            await PopulateFilterOptionsAsync(vm, filter);

            vm.Details = vm
                .Details.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportWirdsExcel(WirdReportFilterDto filter)
        {
            var vm = await _wirdService.GetWirdReportAsync(filter);

            var bytes = WirdReportExcelExporter.Build(vm);
            var fileName = $"wird-report-{DateTime.Today:yyyy-MM-dd}.xlsx";
            return File(bytes, WirdReportExcelExporter.ContentType, fileName);
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
            WirdReportFilterDto filter
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

            var institutes = await _instituteService.GetAllAsync();
            vm.InstituteOptions = institutes
                .OrderBy(i => i.Name)
                .Select(i => new SelectOption(i.Id.ToString(), i.Name))
                .ToList();
        }
    }
}

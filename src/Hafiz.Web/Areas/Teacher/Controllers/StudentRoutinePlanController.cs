using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs.StudentPlan;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hafiz.Web.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class StudentRoutinePlanController : Controller
    {
        private readonly IStudentRoutinePlanService _planService;
        private readonly ILogger<StudentRoutinePlanController> _logger;

        public StudentRoutinePlanController(
            IStudentRoutinePlanService planService,
            ILogger<StudentRoutinePlanController> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// استرجاع خطة الطالب الحالية لتعبئة الـ Modal في الواجهة
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlanByStudentId(Guid studentId)
        {
            try
            {
                var plan = await _planService.GetPlanByStudentIdAsync(studentId);
                return Json(new { success = true, data = plan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching routine plan for student: {StudentId}", studentId);
                return Json(new { success = false, message = "حدث خطأ أثناء جلب الخطة الروتينية للطالب" });
            }
        }

        /// <summary>
        /// استرجاع خطط جميع طلاب حلقة محددة (لتسريع التحميل عند فتح صفحة الحلقة)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlansByClassId(Guid classId)
        {
            try
            {
                var plans = await _planService.GetPlansByClassIdAsync(classId);
                return Json(new { success = true, data = plans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching routine plans for class: {ClassId}", classId);
                return Json(new { success = false, message = "حدث خطأ أثناء جلب خطط الحلقة" });
            }
        }

        /// <summary>
        /// حفظ أو تحديث خطة الطالب (Upsert)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePlan([FromBody] UpsertStudentRoutinePlanDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var (success, message) = await _planService.SetPlanAsync(model);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving routine plan for student: {StudentId}", model?.StudentId);
                return Json(new { success = false, message = "حدث خطأ غير متوقع أثناء حفظ الخطة" });
            }
        }

        /// <summary>
        /// حذف الخطة المخصصة للطالب وإعادته إلى الوضع الافتراضي العام
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeletePlan(Guid studentId)
        {
            try
            {
                var (success, message) = await _planService.DeletePlanAsync(studentId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting routine plan for student: {StudentId}", studentId);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الخطة" });
            }
        }
    }
}


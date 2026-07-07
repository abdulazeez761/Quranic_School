using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Dashboard;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        // حجم صفحة قائمة النشاطات — يجب أن يطابق DashboardService.ActivityPageSize
        // حتى تتوافق أوّل صفحة (تُحمَّل مع الصفحة) مع بقية الصفحات (تُحمَّل عبر AJAX).
        private const int ActivityPageSize = 10;

        [HttpGet]
        public async Task<IActionResult> Index(DashboardPeriod period = DashboardPeriod.AllTime)
        {
            var instituteId = GetInstituteId();
            DashboardStatsDto stats = await _dashboardService.GetDashboardStatsAsync(
                instituteId,
                period
            );
            return View(stats);
        }

        /// <summary>
        /// نقطة نهاية AJAX لزر "تحميل المزيد" في قسم نشاط اليوم (أوراد / حضور).
        /// تُرجع جزءًا HTML يحتوي على عناصر الصفحة الجديدة فقط مع خصائص data للتحديث.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoadActivity(
            DashboardActivityCategory category,
            int page = 1
        )
        {
            var instituteId = GetInstituteId();
            var result = await _dashboardService.GetActivityPageAsync(
                instituteId,
                category,
                page,
                ActivityPageSize
            );
            return PartialView("_ActivityItems", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

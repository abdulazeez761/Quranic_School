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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instituteId = GetInstituteId();
            DashboardStatsDto stats = await _dashboardService.GetDashboardStatsAsync(instituteId);
            return View(stats);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

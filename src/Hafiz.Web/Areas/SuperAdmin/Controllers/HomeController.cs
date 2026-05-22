using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class HomeController : Controller
    {
        private readonly IInstituteService _instituteService;

        public HomeController(IInstituteService instituteService)
        {
            _instituteService = instituteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var institutes = await _instituteService.GetAllAsync();
            return View(institutes);
        }
    }
}

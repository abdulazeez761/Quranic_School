using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITeacherService _teacherService;
        private readonly IClassService _classService;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(
            IAuthService authService,
            ITeacherService teacherService,
            IClassService classService,
            ILogger<TeachersController> logger
        )
        {
            _authService = authService;
            _teacherService = teacherService;
            _classService = classService;
            _logger = logger;
        }

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        public async Task<IActionResult> Index()
        {
            var instituteId = GetInstituteId();
            IEnumerable<Models.Teacher> list;

            if (instituteId.HasValue)
                list = await _teacherService.GetAllTeachersByInstituteAsync(instituteId.Value);
            else
                list = await _teacherService.GetAllTeachersAsync();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateClassesDropdown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClassesDropdown();
                return View(model);
            }
            var instituteId = GetInstituteId();
            var (Success, ErrorMessage) = _authService.RegisterAsync(model, instituteId).Result;
            if (!Success)
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
                await PopulateClassesDropdown();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            var teacher = await _teacherService.GetTeacherByIDAsync(id);

            if (teacher == null)
            {
                ModelState.AddModelError(string.Empty, "Teacher not found");
                return View();
            }
            else
                return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _teacherService.UpdateTeacherAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _teacherService.DeleteTeacherAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateClassesDropdown()
        {
            var instituteId = GetInstituteId();
            IEnumerable<ClassDto> classes;

            if (instituteId.HasValue)
                classes = await _classService.GetClassesByInstituteAsync(instituteId.Value);
            else
                classes = await _classService.GetClassesAsync();

            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
        }
    }
}

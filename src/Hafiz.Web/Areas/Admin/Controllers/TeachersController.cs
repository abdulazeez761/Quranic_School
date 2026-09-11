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

        public async Task<IActionResult> Index(bool archived = false)
        {
            var instituteId = GetInstituteId();
            IEnumerable<Models.Teacher> list;

            if (archived)
            {
                list = instituteId.HasValue
                    ? await _teacherService.GetArchivedTeachersByInstituteAsync(instituteId.Value)
                    : await _teacherService.GetArchivedTeachersAsync();
            }
            else
            {
                list = instituteId.HasValue
                    ? await _teacherService.GetAllTeachersByInstituteAsync(instituteId.Value)
                    : await _teacherService.GetAllTeachersAsync();
            }

            var (activeCount, archivedCount) = await _teacherService.GetCountsAsync(instituteId);
            ViewBag.IsArchived = archived;
            ViewBag.ActiveCount = activeCount;
            ViewBag.ArchivedCount = archivedCount;

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
            var instituteId = GetInstituteId();
            var teacher = await _teacherService.GetTeacherByIDAsync(id, instituteId);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found or not authorized.";
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var instituteId = GetInstituteId();
            var existingTeacher = await _teacherService.GetTeacherByIDAsync(model.Id, instituteId);
            if (existingTeacher == null)
                return Forbid();

            await _teacherService.UpdateTeacherAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var instituteId = GetInstituteId();
            var deleted = await _teacherService.DeleteTeacherAsync(id, instituteId);
            if (!deleted)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بحذف هذا المعلم أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت أرشفة المعلم بنجاح، ويمكنك استعادته في أي وقت من قسم الأرشيف.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var instituteId = GetInstituteId();
            var restored = await _teacherService.RestoreTeacherAsync(id, instituteId);
            if (!restored)
            {
                TempData["ErrorMessage"] = "غير مصرح لك باستعادة هذا المعلم أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت استعادة المعلم بنجاح وإعادة تفعيله.";
            }
            return RedirectToAction(nameof(Index), new { archived = true });
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

using System;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.DTOs;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class InstitutesController : Controller
    {
        private readonly IInstituteService _instituteService;

        public InstitutesController(IInstituteService instituteService)
        {
            _instituteService = instituteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool archived = false)
        {
            List<Institute> institutes;
            if (archived)
            {
                institutes = await _instituteService.GetArchivedInstitutesAsync();
            }
            else
            {
                institutes = await _instituteService.GetAllAsync();
            }

            var (activeCount, archivedCount) = await _instituteService.GetCountsAsync();
            ViewBag.IsArchived = archived;
            ViewBag.ActiveCount = activeCount;
            ViewBag.ArchivedCount = archivedCount;

            return View(institutes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateInstituteDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInstituteDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, errorMessage) = await _instituteService.CreateInstituteWithAdminAsync(
                dto
            );

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                return View(dto);
            }

            TempData["SuccessMessage"] = "تم إنشاء المركز بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var institute = await _instituteService.GetByIdAsync(id);
            if (institute == null)
                return NotFound();

            ViewBag.StudentCount = await _instituteService.GetStudentCountAsync(id);
            ViewBag.TeacherCount = await _instituteService.GetTeacherCountAsync(id);
            ViewBag.ClassCount = await _instituteService.GetClassCountAsync(id);
            ViewBag.Admins = await _instituteService.GetInstituteAdminsAsync(id);

            return View(institute);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _instituteService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تمت أرشفة المركز بنجاح، ويمكنك استعادته من قسم الأرشيف.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var restored = await _instituteService.RestoreInstituteAsync(id);
            if (!restored)
            {
                TempData["ErrorMessage"] = "تعذر استعادة المركز أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت استعادة المركز بنجاح وإعادة تفعيله.";
            }
            return RedirectToAction(nameof(Index), new { archived = true });
        }
    }
}

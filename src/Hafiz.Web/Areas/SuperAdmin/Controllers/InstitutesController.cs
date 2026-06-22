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
        public async Task<IActionResult> Index()
        {
            var institutes = await _instituteService.GetAllAsync();
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
            TempData["SuccessMessage"] = "تم حذف المركز بنجاح!";
            return RedirectToAction(nameof(Index));
        }
    }
}

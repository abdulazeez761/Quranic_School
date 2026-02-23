using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ClassesController : Controller
    {
        private readonly IClassService _ClassService;
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentsService;

        public ClassesController(
            IClassService classService,
            ITeacherService teacherService,
            IStudentService studentService
        )
        {
            _ClassService = classService;
            _teacherService = teacherService;
            _studentsService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Class>? classes = await _ClassService.ViewClasses();

            return View(classes);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateTeachersDropdown();
            await PopulateStudentsDropDown();
            return View();
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateClassDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTeachersDropdown();
                await PopulateStudentsDropDown();
                return View(dto);
            }

            try
            {
                await _ClassService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء الحلقة: " + ex.Message);
                await PopulateTeachersDropdown();
                await PopulateStudentsDropDown();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var classToEdit = await _ClassService.GetClassById(id);
            await PopulateTeachersDropdown();
            await PopulateStudentsDropDown();
            return View(classToEdit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ClassDto classDto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTeachersDropdown();
                await PopulateStudentsDropDown();
                return View(classDto);
            }

            var updated = await _ClassService.UpdateAsync(classDto);
            if (!updated)
            {
                await PopulateTeachersDropdown();
                await PopulateStudentsDropDown();
                ModelState.AddModelError("", "Failed to update class. Please try again.");
                return View(classDto);
            }

            TempData["SuccessMessage"] = "Class updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _ClassService.DeleteClass(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTeachersDropdown()
        {
            // Fetch all teachers from your repository
            var teachers = await _teacherService.GetAllTeachersAsync();

            // Create SelectListItem collection
            ViewBag.Teachers = teachers
                .Select(t => new SelectListItem
                {
                    Value = t.UserId.ToString(),
                    Text = $"{t.TeacherInfo.FirstName} {t.TeacherInfo.SecondName}",
                })
                .ToList();
        }

        private async Task PopulateStudentsDropDown()
        {
            // Fetch all classes from your repository
            var students = await _studentsService.GetAllAsync();

            // Create SelectListItem collection
            ViewBag.Students = students
                .Select(stude => new SelectListItem
                {
                    Value = stude.UserId.ToString(),
                    Text = $"{stude.StudentInfo.FirstName} {stude.StudentInfo.SecondName}",
                })
                .ToList();
        }
    }
}

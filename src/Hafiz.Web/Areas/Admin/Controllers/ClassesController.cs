using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instituteId = GetInstituteId();
            IEnumerable<Class> classes;

            if (instituteId.HasValue)
                classes = await _ClassService.ViewClassesByInstitute(instituteId.Value);
            else
                classes = await _ClassService.ViewClasses();

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
                var instituteId = GetInstituteId();
                await _ClassService.CreateAsync(dto, instituteId);
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
            var instituteId = GetInstituteId();
            IEnumerable<Hafiz.Models.Teacher> teachers;

            if (instituteId.HasValue)
                teachers = await _teacherService.GetAllTeachersByInstituteAsync(instituteId.Value);
            else
                teachers = await _teacherService.GetAllTeachersAsync();

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
            var instituteId = GetInstituteId();
            IEnumerable<Hafiz.Models.Student> students;

            if (instituteId.HasValue)
                students = await _studentsService.GetAllByInstituteAsync(instituteId.Value);
            else
                students = await _studentsService.GetAllAsync();

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

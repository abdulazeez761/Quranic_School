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
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(bool archived = false)
        {
            var instituteId = GetInstituteId();
            IEnumerable<Class> classes;

            if (archived)
            {
                classes = instituteId.HasValue
                    ? await _ClassService.GetArchivedClassesByInstituteAsync(instituteId.Value)
                    : await _ClassService.GetArchivedClassesAsync();
            }
            else
            {
                classes = instituteId.HasValue
                    ? await _ClassService.ViewClassesByInstitute(instituteId.Value)
                    : await _ClassService.ViewClasses();
            }

            var (activeCount, archivedCount) = await _ClassService.GetCountsAsync(instituteId);
            ViewBag.IsArchived = archived;
            ViewBag.ActiveCount = activeCount;
            ViewBag.ArchivedCount = archivedCount;

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
            var instituteId = GetInstituteId();
            var classToEdit = await _ClassService.GetClassById(id, instituteId);
            if (classToEdit == null)
                return NotFound();

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

            var instituteId = GetInstituteId();
            if (classDto.Id.HasValue)
            {
                var existingClass = await _ClassService.GetClassById(
                    classDto.Id.Value,
                    instituteId
                );
                if (existingClass == null)
                    return Forbid();
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
            try
            {
                var instituteId = GetInstituteId();
                var deleted = await _ClassService.DeleteClass(id, instituteId);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "غير مصرح لك بحذف هذه الشعبة أو أنها غير موجودة.";
                }
                else
                {
                    TempData["SuccessMessage"] = "تمت أرشفة الشعبة بنجاح، ويمكنك استعادتها في أي وقت من قسم الأرشيف.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "تعذر حذف الشعبة نظراً لوجود بيانات مرتبطة بها.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "حدث خطأ غير متوقع أثناء محاولة الحذف، يرجى المحاولة لاحقاً.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var instituteId = GetInstituteId();
            var restored = await _ClassService.RestoreClassAsync(id, instituteId);
            if (!restored)
            {
                TempData["ErrorMessage"] = "غير مصرح لك باستعادة هذه الشعبة أو أنها غير موجودة.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت استعادة الشعبة بنجاح وإعادة تفعيلها.";
            }
            return RedirectToAction(nameof(Index), new { archived = true });
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using StudentModel = Hifz.Models.Student;

namespace Hifz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly IParentService _parentService;

        public StudentsController(
            IAuthService authService,
            IStudentService studentService,
            IClassService classService,
            IParentService parentService
        )
        {
            _authService = authService;
            _studentService = studentService;
            _classService = classService;
            _parentService = parentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<StudentModel>? students = await _studentService.GetAllAsync();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateClassesDropdown();
            await PopulateParentsDropdown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterStudentDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(registerDto);
            }
            var (Success, ErrorMessage) = await _studentService.AddAsync(registerDto);
            if (!Success)
            {
                ModelState.AddModelError("", ErrorMessage);
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(registerDto);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _studentService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student is null)
                return NotFound();
            EditStudentDto editStudentDto = new EditStudentDto
            {
                StudentID = id,
                FirstName = student.FirstName,
                SecondName = student.SecondName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                ClassId = student.ClassId,
                ParentId = student.ParentId,
                MemorizedJuz = student.MemorizedJuz,
                TajwidLevel = student.TajwidLevel,
                sex = student.sex,
                ClassesIds = student.ClassesIds,
            };

            await PopulateParentsDropdown();
            await PopulateClassesDropdown();
            return View(editStudentDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditStudentDto newData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateClassesDropdown();
                    await PopulateParentsDropdown();
                    return View(newData);
                }
                await _studentService.UpdateAsync(newData);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateClassesDropdown();
                await PopulateParentsDropdown();
                return View(newData);
            }
        }

        private async Task PopulateClassesDropdown()
        {
            // Fetch all classes from your repository
            var classes = await _classService.GetClassesAsync();

            // Create SelectListItem collection
            ViewBag.Classes = classes
                .Select(cl => new SelectListItem { Value = cl.Id.ToString(), Text = $"{cl.Name}" })
                .ToList();
        }

        private async Task PopulateParentsDropdown()
        {
            // Fetch all parents from the repository
            var parents = await _parentService.GetAllAsync();

            // Create SelectListItem collection
            ViewBag.Parents = parents
                .Select(p => new SelectListItem 
                { 
                    Value = p.UserId.ToString(), 
                    Text = $"{p.ParentInfo.FirstName} {p.ParentInfo.SecondName}" 
                })
                .ToList();
        }
    }
}

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hifz.Models.Student;

namespace Hifz.Areas.Student.Controllers
{
    [Authorize(Roles = "Student")]
    [Area("Student")]
    public class ProfileController : Controller
    {
        private readonly IStudentService _studentService;

        public ProfileController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            StudentModel? student = await _studentService.GetStudentByUserIdAsync(userId);
            
            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            return View(student);
        }
    }
}

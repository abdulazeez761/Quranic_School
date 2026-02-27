using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.DTOs.Student;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;
        private readonly IStudentService _studentService;
        private readonly IWirdService _wirdService;
        private readonly IParentNoteService _parentNoteService;

        public StudentController(
            ILogger<StudentController> logger,
            IStudentService studentService,
            IWirdService wirdService,
            IParentNoteService parentNoteService
        )
        {
            _logger = logger;
            _studentService = studentService;
            _wirdService = wirdService;
            _parentNoteService = parentNoteService;
        }

        public async Task<IActionResult> Index()
        {
            // _ = Guid.Parse(
            //     User.FindFirstValue(ClaimTypes.NameIdentifier)! //always exist because he cant teach the page if he is not logged in
            // );
            string? selectedClassFromCookies = Request.Cookies["selectedClassId"];
            Guid? selectedClass;
            if (selectedClassFromCookies is not null)
                selectedClass = Guid.Parse(selectedClassFromCookies);
            else
            {
                ModelState.AddModelError(string.Empty, "");
                return View(new List<StudentModel>());
            }

            IEnumerable<StudentModel> students = await _studentService.GetStudentsByClassID(
                selectedClass
            );
            return View(students);
        }

        public async Task<IActionResult> Details(Guid id, int page = 1)
        {
            const int pageSize = 5;

            try
            {
                StudentModel? student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                // Get parent notes for this student
                var parentNotes = await _parentNoteService.GetNotesByStudentIdAsync(id);
                ViewBag.ParentNotes = parentNotes;

                // Calculate pagination
                int totalWirds = student.wirds?.Count ?? 0;
                int totalPages = (int)Math.Ceiling((double)totalWirds / pageSize);

                // Validate page number
                if (page < 1)
                    page = 1;
                if (page > totalPages && totalPages > 0)
                    page = totalPages;

                // Get paginated Wirds
                List<WirdAssignment> paginatedWirds =
                    student
                        .wirds?.OrderByDescending(w => w.AssignedDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList() ?? new();

                // Create view model
                var viewModel = new StudentDetailsViewModel
                {
                    Student = student,
                    PaginatedWirds = paginatedWirds,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalWirds = totalWirds,
                    PageSize = pageSize,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student details for ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while retrieving student details.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> AssignWird(WirdAssignment model)
        {
            (bool isAdded, string message) = await _wirdService.AddWirdAsync(model);

            if (isAdded)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Index", "Student", new { area = "Teacher" });
        }

        [HttpPost]
        public async Task<IActionResult> EditWird(WirdAssignment model)
        {
            // (bool isUpdated, string message) = await _wirdService.UpdateWirdAsync(model);

            // if (isUpdated)
            //     TempData["SuccessMessage"] = message;
            // else
            //     TempData["ErrorMessage"] = message;

            return RedirectToAction("Index", "Wird", new { area = "Teacher" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}

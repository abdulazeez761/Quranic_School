using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInstituteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _instituteService.CreateInstituteWithAdminAsync(
                model.InstituteName, model.Description, model.Address, model.InstitutePhone,
                model.AdminUsername, model.AdminFirstName, model.AdminSecondName,
                model.AdminPhoneNumber, model.AdminEmail, model.AdminPassword);

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
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

    public class CreateInstituteViewModel
    {
        // Institute fields
        [Required(ErrorMessage = "اسم المركز مطلوب")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "اسم المركز")]
        public string InstituteName { get; set; }

        [StringLength(500)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "هاتف المركز")]
        public string? InstitutePhone { get; set; }

        // Admin fields
        [Required(ErrorMessage = "اسم المستخدم للمدير مطلوب")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "اسم المستخدم يمكن أن يحتوي فقط على حروف وأرقام وشرطات")]
        [Display(Name = "اسم المستخدم")]
        public string AdminUsername { get; set; }

        [Required(ErrorMessage = "الاسم الأول للمدير مطلوب")]
        [StringLength(50)]
        [Display(Name = "الاسم الأول")]
        public string AdminFirstName { get; set; }

        [Required(ErrorMessage = "اسم العائلة للمدير مطلوب")]
        [StringLength(50)]
        [Display(Name = "اسم العائلة")]
        public string AdminSecondName { get; set; }

        [Required(ErrorMessage = "رقم هاتف المدير مطلوب")]
        [Phone]
        [StringLength(20)]
        [Display(Name = "رقم الهاتف")]
        public string AdminPhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "البريد الإلكتروني")]
        public string? AdminEmail { get; set; }

        [Required(ErrorMessage = "كلمة مرور المدير مطلوبة")]
        [MinLength(6)]
        [Display(Name = "كلمة المرور")]
        public string AdminPassword { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("AdminPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string AdminConfirmPassword { get; set; }
    }
}

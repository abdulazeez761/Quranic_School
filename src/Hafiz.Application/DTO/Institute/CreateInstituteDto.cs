using System.ComponentModel.DataAnnotations;

namespace Hafiz.DTOs
{
    public class CreateInstituteDto
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
        [RegularExpression(
            @"^[a-zA-Z0-9_-]+$",
            ErrorMessage = "اسم المستخدم يمكن أن يحتوي فقط على حروف وأرقام وشرطات"
        )]
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

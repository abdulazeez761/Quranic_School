using System.ComponentModel.DataAnnotations;

namespace Hifz.DTOs
{
    public class TeacherDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول لا يمكن أن يزيد عن 50 حرفًا")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الثاني مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الثاني لا يمكن أن يزيد عن 50 حرفًا")]
        [Display(Name = "الاسم الثاني")]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [StringLength(20, ErrorMessage = "رقم الهاتف لا يمكن أن يزيد عن 20 رقمًا")]
        [RegularExpression(
            @"^(?:\+962|0)?7[789]\d{7}$",
            ErrorMessage = "يرجى إدخال رقم هاتف أردني صالح"
        )]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "اسم المستخدم يجب أن يكون بين 3 و 50 حرفًا")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "اسم المستخدم يمكن أن يحتوي فقط على حروف وأرقام وشرطات")]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "كلمة المرور يجب أن تتكون من 8 أحرف على الأقل"
        )]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
            ErrorMessage = "ادخل كلمة مرور تحتوي على حرف كبير واحد على الأقل، وحرف صغير واحد، ورقم واحد، وحرف خاص واحد."
        )]
        public string? Password { get; set; }

        // If you need the teacher ID for updating
        [Required]
        public Guid Id { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Hafiz.DTOs
{
    public class TeacherDto
    {
        [Required(ErrorMessage = "FirstNameRequired")]
        [StringLength(50, ErrorMessage = "FirstNameLength")]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "SecondNameRequired")]
        [StringLength(50, ErrorMessage = "SecondNameLength")]
        [Display(Name = "SecondName")]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "PhoneRequired")]
        [StringLength(20, ErrorMessage = "PhoneLength")]
        [RegularExpression(@"^(?:\+962|0)?7[789]\d{7}$", ErrorMessage = "PhoneFormat")]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "UsernameRequired")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "UsernameLength")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "UsernameFormat")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "EmailFormat")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "PasswordLength")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
            ErrorMessage = "PasswordFormat"
        )]
        public string? Password { get; set; }

        // If you need the teacher ID for updating
        [Required]
        public Guid Id { get; set; }
    }
}

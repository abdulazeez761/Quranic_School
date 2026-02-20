using System;
using System.ComponentModel.DataAnnotations;

namespace Hafiz.DTOs
{
    public class EditParentDto
    {
        public Guid? ParentID { get; set; }

        [StringLength(50, ErrorMessage = "FirstNameLength")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "SecondNameLength")]
        public string? SecondName { get; set; }

        [Phone]
        [StringLength(20, ErrorMessage = "PhoneLength")]
        public string? PhoneNumber { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "UsernameLength")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "UsernameFormat")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "EmailFormat")]
        [StringLength(100)]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "PasswordMinLength")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
            ErrorMessage = "PasswordFormat"
        )]
        public string? Password { get; set; }
    }
}

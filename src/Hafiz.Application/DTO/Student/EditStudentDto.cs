using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs
{
    public class EditStudentDto
    {
        public Guid? StudentID { get; set; }

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

        // Optional fields based on role
        public Guid? ClassId { get; set; } // Student

        public Guid? ParentId { get; set; }

        [Range(0, 31)]
        public int MemorizedJuz { get; set; } // Student

        [Range(0, 4)]
        public TajwidLevel? TajwidLevel { get; set; } // Student

        public Sex sex { get; set; }

        [Required(ErrorMessage = "DateOfBirthRequired")]
        [DataType(DataType.Date)]
        [Display(Name = "DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        public List<Guid>? ClassesIds { get; set; } = new();
    }
}

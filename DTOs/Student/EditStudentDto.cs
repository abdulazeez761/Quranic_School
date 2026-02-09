using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;
using Hifz.Models.enums;

namespace Hifz.DTOs
{
    public class EditStudentDto
    {
        public Guid? StudentID { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? SecondName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
        public string? Username { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [MinLength(6)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
            ErrorMessage = "Password must be at least 6 characters long, contain at least one uppercase letter, one lowercase letter, and one number."
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

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public List<Guid>? ClassesIds { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs
{
    public class RegisterStudentDto : RegisterDto
    {
        // Optional fields based on role
        public Guid? ClassId { get; set; } // Student

        public Guid? ParentId { get; set; }

        [Range(0, 30)]
        public int MemorizedJuz { get; set; } // Student

        [Range(0, 3)]
        public TajwidLevel? TajwidLevel { get; set; } // Student

        public Sex sex { get; set; }

        [Required(ErrorMessage = "DateOfBirthRequired")]
        [DataType(DataType.Date)]
        [Display(Name = "DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        public List<Guid>? ClassesIds { get; set; } = new();
    }
}

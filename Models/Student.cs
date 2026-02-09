using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hifz.Models.enums;

namespace Hifz.Models
{
    public class Student
    {
        [Key]
        [Required(ErrorMessage = "User ID is required.")]
        [ForeignKey("StudentInfo")]
        public Guid UserId { get; set; }

        public Guid? ClassId { get; set; }

        [ForeignKey("Parent")]
        public Guid? ParentId { get; set; }

        [Required(ErrorMessage = "Memorized Juz is required.")]
        [Range(0, 30, ErrorMessage = "Memorized Juz must be between 0 and 30.")]
        public int MemorizedJuz { get; set; }

        [Required(ErrorMessage = "Tajwid Level is required.")]
        [Display(Name = "Tajwid Level")]
        public TajwidLevel TajwidLevel { get; set; }

        [Required]
        public Sex sex { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Student Info")]
        [ForeignKey("UserId")]
        public User StudentInfo { get; set; }

        [ForeignKey("ParentId")]
        public Parent? Parent { get; set; }

        [Display(Name = "Assigned Classes")]
        public ICollection<Class> Classes { get; set; } = new List<Class>();

        public ICollection<StudentAttendance> Attendances { get; set; } =
            new List<StudentAttendance>();

        public ICollection<WirdAssignment> wirds { get; set; } = new List<WirdAssignment>();
    }
}

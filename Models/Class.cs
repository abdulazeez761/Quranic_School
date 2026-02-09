using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hifz.Models.enums;

namespace Hifz.Models;

public class Class
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Class name is required.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "The name cannot exceed 100 characters and less than 3 char."
    )]
    public string Name { get; set; }

    [Required(ErrorMessage = "class Gender is required.")]
    public Sex Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Student> Students { get; set; } = new List<Student>();

    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<StudentAttendance> StudentAttendances { get; set; } =
        new List<StudentAttendance>();
    public ICollection<TeacherAttendance> TeacherAttendance { get; set; } =
        new List<TeacherAttendance>();
    public ICollection<ClassDaysEnum> ClassDays { get; set; } = new List<ClassDaysEnum>();
    public DateTime ClassTime { get; set; }

    public bool IsMeetingActive { get; set; } = false;
}

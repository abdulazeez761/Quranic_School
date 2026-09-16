using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Entities;
using Hafiz.Models.enums;

namespace Hafiz.Models;

public class Class : ISoftDeletable
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

    public Guid? InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute? Institute { get; set; }

    // الربط بالبرنامج التعليمي
    public Guid? StudyProgramId { get; set; }

    [ForeignKey(nameof(StudyProgramId))]
    public StudyProgram? StudyProgram { get; set; }
    public ICollection<StudentAttendance> StudentAttendances { get; set; } =
        new List<StudentAttendance>();
    public ICollection<TeacherAttendance> TeacherAttendance { get; set; } =
        new List<TeacherAttendance>();
    public ICollection<ClassDaysEnum> ClassDays { get; set; } = new List<ClassDaysEnum>();
    public DateTime ClassTime { get; set; }

    public bool IsMeetingActive { get; set; } = false;

    // Implementation of ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs;

public class StudentDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
    public string Username { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string SecondName { get; set; }

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    // Optional fields based on role
    public Guid? ClassId { get; set; } // Student

    public Guid? ParentId { get; set; }

    // public Guid? ParentId { get; set; } // Student

    [Range(0, 30)]
    [Required]
    public int MemorizedJuz { get; set; } // Student

    [Range(0, 3)]
    public TajwidLevel? TajwidLevel { get; set; } // Student

    [Required]
    public Sex sex { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    public List<Guid>? ClassesIds { get; set; } = new();
}

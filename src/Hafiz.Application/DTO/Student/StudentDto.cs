using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs;

public class StudentDto
{
    [Required(ErrorMessage = "UsernameRequired")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "UsernameLength")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "UsernameFormat")]
    public string Username { get; set; }

    [Required(ErrorMessage = "FirstNameRequired")]
    [StringLength(50, ErrorMessage = "FirstNameLength")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "SecondNameRequired")]
    [StringLength(50, ErrorMessage = "SecondNameLength")]
    public string SecondName { get; set; }

    [Required(ErrorMessage = "PhoneRequired")]
    [Phone]
    [StringLength(20, ErrorMessage = "PhoneLength")]
    public string PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "EmailFormat")]
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

    [Required(ErrorMessage = "DateOfBirthRequired")]
    [DataType(DataType.Date)]
    [Display(Name = "DateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    public List<Guid>? ClassesIds { get; set; } = new();
}

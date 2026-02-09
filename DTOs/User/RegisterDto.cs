using System;
using System.ComponentModel.DataAnnotations;
using Hifz.Models;

namespace Hifz.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
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
    [MinLength(6)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
        ErrorMessage = "Password must be at least 6 characters long, contain at least one uppercase letter, one lowercase letter, and one number."
    )]
    public string Password { get; set; }

    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    public UserRole Role { get; set; }
}

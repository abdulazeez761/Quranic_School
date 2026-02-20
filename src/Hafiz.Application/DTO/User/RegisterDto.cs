using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Models;

namespace Hafiz.DTOs;

public class RegisterDto
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

    [Required(ErrorMessage = "PasswordRequired")]
    [MinLength(6, ErrorMessage = "PasswordMinLength")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
        ErrorMessage = "PasswordFormat"
    )]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "PasswordMismatch")]
    public string ConfirmPassword { get; set; }

    [Required]
    public UserRole Role { get; set; }
}

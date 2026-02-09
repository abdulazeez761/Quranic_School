using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Hifz.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Username is Required")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
        ErrorMessage = "PasswordRegex"
    )]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

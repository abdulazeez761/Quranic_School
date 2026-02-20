using System.ComponentModel.DataAnnotations;

namespace Hafiz.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "UsernameRequired")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "UsernameLength")]
    public string Username { get; set; }

    [Required(ErrorMessage = "PasswordRequired")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|])[A-Za-z\d!@#$%^&*()_+{}\[\]:;<>,.?~\-=/\\|]{8,}$",
        ErrorMessage = "PasswordFormat"
    )]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

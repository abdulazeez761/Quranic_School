using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
        public string Username { get; set; }

        [Required]
        [StringLength(50)] // Max length for first name
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)] // Max length for second name
        public string SecondName { get; set; }

        [Required]
        [Phone] // Validates phone format loosely
        [StringLength(20)] // Max length for phone number
        public string PhoneNumber { get; set; }

        [EmailAddress] // Validates email format
        [StringLength(100)] // Max length for email
        public string? Email { get; set; } // Optional, kept for contact purposes

        [Required]
        [StringLength(200)] // You might store hashed passwords, so length can vary
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ProfileImage { get; set; }

        // Navigation properties

        public Teacher? Teacher { get; set; }

        public Student? Student { get; set; }

        public Parent? Parent { get; set; }
    }
}

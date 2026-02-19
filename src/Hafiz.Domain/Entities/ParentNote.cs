using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hafiz.Models
{
    public class ParentNote
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Created by is required.")]
        [ForeignKey("CreatedByUser")]
        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Models;

namespace Hafiz.Domain.Entities
{
    public class Institute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? ManagerId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(ManagerId))]
        public User? Manager { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;

namespace Hafiz.Models
{
    public class Parent : ISoftDeletable
    {
        [Key]
        [Required(ErrorMessage = "User ID is required.")]
        [ForeignKey("ParentInfo")]
        public Guid UserId { get; set; }

        [Display(Name = "Parent Info")]
        [ForeignKey("UserId")]
        public User ParentInfo { get; set; }

        [Display(Name = "Children")]
        public ICollection<Student> Students { get; set; } = new List<Student>();

        // Implementation of ISoftDeletable
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}

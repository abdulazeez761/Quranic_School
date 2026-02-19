using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hafiz.Models
{
    public class Parent
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
    }
}

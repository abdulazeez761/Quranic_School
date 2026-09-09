using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;

namespace Hafiz.Models
{
    public class Teacher : ISoftDeletable
    {
        [Key]
        [Required(ErrorMessage = "User ID is required.")]
        [ForeignKey("TeacherInfo")]
        public Guid UserId { get; set; }

        [Display(Name = "Teacher Info")]
        [ForeignKey("UserId")]
        public User TeacherInfo { get; set; }

        [Display(Name = "Assigned Classes")]
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<TeacherAttendance> Attendances { get; set; } =
            new LinkedList<TeacherAttendance>();

        // Implementation of ISoftDeletable
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}

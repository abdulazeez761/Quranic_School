using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hifz.Models
{
    public class Teacher
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
    }
}

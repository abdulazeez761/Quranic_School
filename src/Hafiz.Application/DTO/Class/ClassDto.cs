using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs
{
    public class ClassDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "ClassNameRequired")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ClassNameLength")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ClassGenderRequired")]
        public Sex Gender { get; set; }

        [Required(ErrorMessage = "ClassDaysRequired")]
        public List<ClassDaysEnum> ClassDays { get; set; } = new List<ClassDaysEnum>();

        public List<Guid>? TeacherIds { get; set; } = new(); //we get the ids then we fetch them in the repo after that we assign them to the class entity
        public List<Guid>? StudentsIds { get; set; } = new(); //we get the ids then we fetch them in the repo after that we assign them to the class entity

        [Required(ErrorMessage = "ClassTimeRequired")]
        public DateTime ClassTime { get; set; }
    }
}

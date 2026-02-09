using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models.enums;

namespace Hifz.DTOs
{
    public class CreateClassDto
    {
        [Required(ErrorMessage = "Class name is required.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "The name must be between 3 and 100 characters."
        )]
        public string Name { get; set; }

        [Required(ErrorMessage = "Class gender is required.")]
        public Sex Gender { get; set; }

        [Required(ErrorMessage = "Class days are required.")]
        public List<ClassDaysEnum> ClassDays { get; set; } = new List<ClassDaysEnum>();

        public List<Guid>? TeacherIds { get; set; } = new(); //we get the ids then we fetch them in the repo after that we assign them to the class entity
        public List<Guid>? StudentsIds { get; set; } = new(); //we get the ids then we fetch them in the repo after that we assign them to the class entity

        [Required(ErrorMessage = "Class time is required.")]
        public DateTime ClassTime { get; set; }
    }
}

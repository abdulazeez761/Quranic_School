using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Models.enums;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.DTOs
{
    public class EditClassDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "ClassNameRequired")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ClassNameLength")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ClassGenderRequired")]
        public Sex Gender { get; set; }

        [Required(ErrorMessage = "ClassDaysRequired")]
        public List<ClassDaysEnum> ClassDays { get; set; } = new List<ClassDaysEnum>();

        public List<Teacher>? Teachers { get; set; } = new();
        public List<StudentModel>? Students { get; set; } = new();

        [Required(ErrorMessage = "ClassTimeRequired")]
        public DateTime ClassTime { get; set; }
    }
}

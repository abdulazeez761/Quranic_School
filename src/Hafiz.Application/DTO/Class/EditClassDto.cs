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

        public List<Teacher>? Teachers { get; set; } = new();
        public List<StudentModel>? Students { get; set; } = new();

        [Required(ErrorMessage = "Class time is required.")]
        public DateTime ClassTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;
using Hifz.Models.enums;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Hifz.DTOs
{
    public class RegisterStudentDto : RegisterDto
    {
        // Optional fields based on role
        public Guid? ClassId { get; set; } // Student

        public Guid? ParentId { get; set; }

        [Range(0, 30)]
        public int MemorizedJuz { get; set; } // Student

        [Range(0, 3)]
        public TajwidLevel? TajwidLevel { get; set; } // Student

        public Sex sex { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public List<Guid>? ClassesIds { get; set; } = new();
    }
}

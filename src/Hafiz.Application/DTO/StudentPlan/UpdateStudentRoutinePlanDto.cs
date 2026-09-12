using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Models;

namespace Hafiz.DTOs.StudentPlan
{
    public class UpdateStudentRoutinePlanDto : UpsertStudentRoutinePlanDto
    {
        public Guid? Id { get; set; }
    }
}

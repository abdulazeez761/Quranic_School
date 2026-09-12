using System;

namespace Hafiz.DTOs.StudentPlan
{
    public class StudentRoutinePlanDto : UpsertStudentRoutinePlanDto
    {
        public Guid Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? StudentName { get; set; }
    }
}

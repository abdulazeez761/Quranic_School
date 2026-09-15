using System;

namespace Hafiz.Application.DTO.StudentWird
{
    public class StudentSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
}

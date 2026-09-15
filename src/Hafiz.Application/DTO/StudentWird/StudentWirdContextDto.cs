using Hafiz.DTOs.StudentPlan;

namespace Hafiz.Application.DTO.StudentWird
{
    public class StudentWirdContextDto
    {
        public StudentSummaryDto Student { get; set; } = null!;
        public StudentRoutinePlanDto? Plan { get; set; }
        public LastWirdsDto LastWirds { get; set; } = new();
        public TodayWirdsDto TodayWirds { get; set; } = new();
    }
}

using Hafiz.DTOs.StudentPlan;

namespace Hafiz.Application.DTO.StudentWird
{
    public class StudentWirdContextResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public StudentSummaryDto? Student { get; set; }
        public StudentRoutinePlanDto? Plan { get; set; }
        public LastWirdsDto? LastWirds { get; set; }
        public TodayWirdsDto? TodayWirds { get; set; }

        public static StudentWirdContextResponseDto Ok(StudentWirdContextDto context) =>
            new()
            {
                Success = true,
                Student = context.Student,
                Plan = context.Plan,
                LastWirds = context.LastWirds,
                TodayWirds = context.TodayWirds
            };

        public static StudentWirdContextResponseDto Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}

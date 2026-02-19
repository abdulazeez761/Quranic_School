using Hafiz.DTOs;

namespace Hafiz.Services.Interfaces
{
    public interface ITeacherAttendanceService
    {
        Task<(bool Success, string ErrorMessage)> AttendTeacher(
            SaveTeacherAttendanceDto saveAttendanceDto
        );

        Task<IEnumerable<TeacherAttendanceDto>> GetTeachersByClass(Guid classId, DateTime date);
    }
}

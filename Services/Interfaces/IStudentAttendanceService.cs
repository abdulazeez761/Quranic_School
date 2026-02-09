using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs.Attendance;

namespace Hifz.Services.Interfaces
{
    public interface IStudentAttendanceService
    {
        Task<(bool Success, string ErrorMessage)> AttendStudent(
            SaveStudentAttendanceDto saveAttendanceDto
        );

        Task<IEnumerable<StudentAttendanceDto>> GetStudentsByClass(Guid classId, DateTime date);
    }
}

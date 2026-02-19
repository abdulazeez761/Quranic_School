using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs.Attendance;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface IStudentAttendanceRepository
    {
        Task AddStudentAttendance(StudentAttendance attendanceInfo);
        Task UpdateStudentAttendance(StudentAttendance attendanceInfo);
        Task<StudentAttendance?> GetAttendanceByClassIdAndDateAndStudentId(
            Guid classId,
            DateTime date,
            Guid StudentId
        );
        Task<IEnumerable<StudentAttendanceDto>> GetStudentsByClassId(Guid classId, DateTime date);
    }
}

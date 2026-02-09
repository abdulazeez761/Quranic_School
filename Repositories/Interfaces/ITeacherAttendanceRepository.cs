using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
{
    public interface ITeacherAttendanceRepository
    {
        Task AddTeacherAttendance(TeacherAttendance attendanceInfo);
        Task UpdateTeacherAttendance(TeacherAttendance attendanceInfo);
        Task<TeacherAttendance?> GetAttendanceByClassIdAndDateAndTeacherId(
            Guid classId,
            DateTime date,
            Guid teacherId
        );
        Task<IEnumerable<TeacherAttendanceDto>> GetTeachersByClass(Guid classId, DateTime date);
    }
}

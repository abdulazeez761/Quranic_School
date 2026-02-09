using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Hifz.Services.Interfaces
{
    public interface ITeacherAttendanceService
    {
        Task<(bool Success, string ErrorMessage)> AttendTeacher(
            SaveTeacherAttendanceDto saveAttendanceDto
        );

        Task<IEnumerable<TeacherAttendanceDto>> GetTeachersByClass(Guid classId, DateTime date);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hifz.Services
{
    public class TeacherAttendanceService : ITeacherAttendanceService
    {
        private readonly ITeacherAttendanceRepository _teacherAttendanceRepository;

        public TeacherAttendanceService(ITeacherAttendanceRepository teacherAttendance)
        {
            _teacherAttendanceRepository = teacherAttendance;
        }

        public async Task<(bool Success, string ErrorMessage)> AttendTeacher(
            SaveTeacherAttendanceDto saveAttendanceDto
        )
        {
            if (saveAttendanceDto == null)
                return (false, "لم يتم تلقي اي بيانات");

            var checkIfAttendanceExist =
                await _teacherAttendanceRepository.GetAttendanceByClassIdAndDateAndTeacherId(
                    saveAttendanceDto.ClassID,
                    saveAttendanceDto.Date,
                    saveAttendanceDto.TeacherId
                );

            TeacherAttendance teacherAttendance = new TeacherAttendance()
            {
                TeacherId = saveAttendanceDto.TeacherId,
                ClassId = saveAttendanceDto.ClassID,
                Date = saveAttendanceDto.Date,
                WorkingHours = saveAttendanceDto.Hours,
                Status = saveAttendanceDto.Status,
            };
            if (checkIfAttendanceExist != null)
            {
                try
                {
                    teacherAttendance.Id = checkIfAttendanceExist.Id;
                    await _teacherAttendanceRepository.UpdateTeacherAttendance(teacherAttendance);
                    return (true, " تم التحديث");
                }
                catch (Exception ex)
                {
                    return (false, "" + ex.Message);
                }
            }

            try
            {
                await _teacherAttendanceRepository.AddTeacherAttendance(teacherAttendance);
                return (true, " تم التسجيل");
            }
            catch (Exception ex)
            {
                return (false, "" + ex.Message);
            }
        }

        public async Task<IEnumerable<TeacherAttendanceDto>> GetTeachersByClass(
            Guid classId,
            DateTime date
        )
        {
            return await _teacherAttendanceRepository.GetTeachersByClass(classId, date);
        }
    }
}

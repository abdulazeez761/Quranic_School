using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs.Attendance;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Hifz.Services.Interfaces;

namespace Hifz.Services
{
    public class StudentAttendanceService : IStudentAttendanceService
    {
        private readonly IStudentAttendanceRepository _studentAttendanceRepository;

        public StudentAttendanceService(IStudentAttendanceRepository studentAttendanceRepository)
        {
            _studentAttendanceRepository = studentAttendanceRepository;
        }

        public async Task<(bool Success, string ErrorMessage)> AttendStudent(
            SaveStudentAttendanceDto saveAttendanceDto
        )
        {
            if (saveAttendanceDto == null)
                return (false, "لم يتم تلقي اي بيانات");

            var checkIfAttendanceExist =
                await _studentAttendanceRepository.GetAttendanceByClassIdAndDateAndStudentId(
                    saveAttendanceDto.ClassID,
                    saveAttendanceDto.Date,
                    saveAttendanceDto.StudentID
                );
            StudentAttendance studentAttendance = new StudentAttendance
            {
                StudentId = saveAttendanceDto.StudentID,
                ClassId = saveAttendanceDto.ClassID,
                Date = saveAttendanceDto.Date,
                Status = saveAttendanceDto.Status,
            };

            if (checkIfAttendanceExist != null)
            {
                try
                {
                    studentAttendance.Id = checkIfAttendanceExist.Id;
                    await _studentAttendanceRepository.UpdateStudentAttendance(studentAttendance);
                    return (true, " تم التحديث");
                }
                catch (Exception ex)
                {
                    return (false, "فشل اثناء تحديث بيانات الطالب" + ex.Message);
                }
            }

            try
            {
                await _studentAttendanceRepository.AddStudentAttendance(studentAttendance);
                return (true, " تم التسجيل");
            }
            catch (Exception ex)
            {
                return (false, "" + ex.Message);
            }
        }

        public async Task<IEnumerable<StudentAttendanceDto>> GetStudentsByClass(
            Guid classId,
            DateTime date
        )
        {
            return await _studentAttendanceRepository.GetStudentsByClassId(classId, date);
        }
    }
}

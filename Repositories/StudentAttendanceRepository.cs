using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Data;
using Hifz.DTOs.Attendance;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Repositories
{
    public class StudentAttendanceRepository : IStudentAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentAttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddStudentAttendance(StudentAttendance attendanceInfo)
        {
            _context.StudentAttendances.Add(attendanceInfo);
            await _context.SaveChangesAsync();
        }

        public async Task<StudentAttendance?> GetAttendanceByClassIdAndDateAndStudentId(
            Guid classId,
            DateTime date,
            Guid StudentId
        )
        {
            var attendance = await _context.StudentAttendances.FirstOrDefaultAsync(att =>
                att.StudentId == StudentId && att.ClassId == classId && att.Date == date
            );
            return attendance;
        }

        public async Task<IEnumerable<StudentAttendanceDto>> GetStudentsByClassId(
            Guid classId,
            DateTime date
        )
        {
            var students = await _context
                .Students.Include(t => t.Attendances)
                .Where(t => t.Classes.Any(c => c.Id == classId))
                .Select(t => new StudentAttendanceDto
                {
                    Id = t.UserId,
                    FirstName = t.StudentInfo.FirstName,
                    SecondName = t.StudentInfo.SecondName,
                    PrevAttendance = t
                        .Attendances.Where(a => a.Date == date.Date && a.ClassId == classId)
                        .Select(a => new PreviousStudentAttendanceDto
                        {
                            StudentId = a.StudentId,
                            Status = (int)a.Status, // convert enum to int
                            ClassId = a.ClassId,
                            Date = a.Date,
                        })
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return students;
        }

        public async Task UpdateStudentAttendance(StudentAttendance attendanceInfo)
        {
            var currentAttendance = await _context.StudentAttendances.FirstOrDefaultAsync(t =>
                t.Id == attendanceInfo.Id
            );
            if (currentAttendance is null)
                return;
            currentAttendance.Status = attendanceInfo.Status;

            await _context.SaveChangesAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Data;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Repositories
{
    public class TeacherAttendanceRepository : ITeacherAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherAttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddTeacherAttendance(TeacherAttendance attendanceInfo)
        {
            _context.teacherAttendances.Add(attendanceInfo);
            return _context.SaveChangesAsync();
        }

        public async Task<TeacherAttendance?> GetAttendanceByClassIdAndDateAndTeacherId(
            Guid classId,
            DateTime date,
            Guid teacherId
        )
        {
            var attendance = await _context.teacherAttendances.FirstOrDefaultAsync(a =>
                a.ClassId == classId && a.Date == date && a.TeacherId == teacherId
            );
            return attendance;
        }

        public async Task<IEnumerable<TeacherAttendanceDto>> GetTeachersByClass(
            Guid classId,
            DateTime date
        )
        {
            var teachers = await _context
                .Teachers.Include(t => t.Attendances)
                .Where(t => t.Classes.Any(cl => cl.Id == classId))
                .Select(t => new TeacherAttendanceDto
                {
                    Id = t.UserId,
                    FirstName = t.TeacherInfo.FirstName,
                    SecondName = t.TeacherInfo.SecondName,
                    PrevAttendance = t
                        .Attendances.Where(a => a.Date == date.Date && a.ClassId == classId)
                        .Select(a => new PreviousTeacherAttendanceDto
                        {
                            TeacherId = a.TeacherId,
                            Status = (int)a.Status, // convert enum to int
                            ClassId = a.ClassId,
                            WorkingHours = a.WorkingHours,
                            Date = a.Date,
                        })
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return teachers;
        }

        public async Task UpdateTeacherAttendance(TeacherAttendance attendanceInfo)
        {
            var currentAtt = await _context.teacherAttendances.FirstOrDefaultAsync(a =>
                a.Id == attendanceInfo.Id
            );
            if (currentAtt is null)
                return;
            currentAtt.Status = attendanceInfo.Status;
            currentAtt.WorkingHours = attendanceInfo.WorkingHours;

            await _context.SaveChangesAsync();
        }
    }
}

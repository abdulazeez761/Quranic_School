using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            return _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _context
                .Teachers.Include(t => t.TeacherInfo)
                .Include(t => t.Classes)
                .Include(t => t.Attendances)
                .ToListAsync();
        }

        public Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            _context.Users.Remove(teacher.TeacherInfo);
            return _context.SaveChangesAsync();
        }

        public async Task<Teacher?> GetTeacherByIDAsync(Guid teacherId)
        {
            Teacher? teacher = _context
                .Teachers.Include(t => t.TeacherInfo)
                .FirstOrDefaultAsync(t => t.UserId == teacherId)
                .Result;
            return teacher;
        }

        public async Task UpdateAsync(TeacherDto teacher)
        {
            var existingTeacher = await _context
                .Teachers.Include(t => t.TeacherInfo)
                .FirstOrDefaultAsync(t => t.UserId == teacher.Id);

            if (existingTeacher != null && teacher != null)
            {
                existingTeacher.TeacherInfo.FirstName =
                    teacher.FirstName ?? existingTeacher.TeacherInfo.FirstName;

                existingTeacher.TeacherInfo.SecondName =
                    teacher.SecondName ?? existingTeacher.TeacherInfo.SecondName;

                existingTeacher.TeacherInfo.PhoneNumber =
                    teacher.PhoneNumber ?? existingTeacher.TeacherInfo.PhoneNumber;

                existingTeacher.TeacherInfo.Username =
                    teacher.Username ?? existingTeacher.TeacherInfo.Username;

                existingTeacher.TeacherInfo.Email =
                    teacher.Email ?? existingTeacher.TeacherInfo.Email;

                existingTeacher.TeacherInfo.Password =
                    teacher.Password ?? existingTeacher.TeacherInfo.Password;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IList<Class>> GetTeacherClasses(Guid teacherId)
        {
            IList<Class> classes = await _context
                .Classes.Include(c => c.Teachers)
                .Include(c => c.Students)
                .Where(c => c.Teachers.Any(t => t.UserId == teacherId))
                .ToListAsync();
            return classes;
        }
    }
}

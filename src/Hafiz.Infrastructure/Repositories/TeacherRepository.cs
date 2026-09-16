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

        public async Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            if (teacher.TeacherInfo != null)
            {
                _context.Users.Remove(teacher.TeacherInfo);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid teacherId, Guid? instituteId = null)
        {
            var query = _context.Teachers
                .Include(t => t.TeacherInfo)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(t => t.TeacherInfo.InstituteId == instituteId.Value);
            }

            var teacher = await query.FirstOrDefaultAsync(t => t.UserId == teacherId);
            if (teacher == null)
                return false;

            _context.Teachers.Remove(teacher);
            if (teacher.TeacherInfo != null)
            {
                _context.Users.Remove(teacher.TeacherInfo);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Teacher?> GetTeacherByIDAsync(Guid teacherId, Guid? instituteId = null)
        {
            var query = _context.Teachers
                .IgnoreQueryFilters()
                .Include(t => t.TeacherInfo)
                .Include(t => t.Classes)
                .Include(t => t.Attendances)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(t => t.TeacherInfo.InstituteId == instituteId.Value);
            }

            return await query.FirstOrDefaultAsync(t => t.UserId == teacherId);
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
                .Include(c => c.StudyProgram)
                .Where(c => c.Teachers.Any(t => t.UserId == teacherId))
                .ToListAsync();
            return classes;
        }

        public async Task<IEnumerable<Teacher>> GetAllByInstituteAsync(Guid instituteId)
        {
            return await _context
                .Teachers.Include(t => t.TeacherInfo)
                .Include(t => t.Classes)
                .Include(t => t.Attendances)
                .Where(t => t.TeacherInfo.InstituteId == instituteId)
                .ToListAsync();
        }

        public async Task<bool> RestoreTeacherAsync(Guid teacherId, Guid? instituteId = null)
        {
            var query = _context.Teachers
                .IgnoreQueryFilters()
                .Include(t => t.TeacherInfo)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(t => t.TeacherInfo.InstituteId == instituteId.Value);
            }

            var teacher = await query.FirstOrDefaultAsync(t => t.UserId == teacherId);

            if (teacher == null || !teacher.IsDeleted)
                return false;
            teacher.IsDeleted = false;
            teacher.DeletedAt = null;
            teacher.DeletedBy = null;
            if (teacher.TeacherInfo != null)
            {
                teacher.TeacherInfo.IsDeleted = false;
                teacher.TeacherInfo.DeletedAt = null;
                teacher.TeacherInfo.DeletedBy = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Teacher>> GetArchivedTeachersByInstituteAsync(Guid instituteId)
        {
            return await _context.Teachers
                .IgnoreQueryFilters()
                .Include(t => t.TeacherInfo)
                .Include(t => t.Classes)
                .Where(t => t.IsDeleted && t.TeacherInfo.InstituteId == instituteId)
                .OrderByDescending(t => t.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetArchivedTeachersAsync()
        {
            return await _context.Teachers
                .IgnoreQueryFilters()
                .Include(t => t.TeacherInfo)
                .Include(t => t.Classes)
                .Where(t => t.IsDeleted)
                .OrderByDescending(t => t.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null)
        {
            var query = _context.Teachers
                .IgnoreQueryFilters()
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(t => t.TeacherInfo.InstituteId == instituteId.Value);
            }

            var activeCount = await query.CountAsync(t => !t.IsDeleted);
            var archivedCount = await query.CountAsync(t => t.IsDeleted);

            return (activeCount, archivedCount);
        }
    }
}

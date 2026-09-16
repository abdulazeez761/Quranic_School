using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Class classInfo)
        {
            await _context.Classes.AddAsync(classInfo);
            foreach (Student student in classInfo.Students)
            {
                Student? studentToUpdate = await _context.Students.FirstOrDefaultAsync(stu =>
                    stu.UserId == student.UserId
                );
                if (studentToUpdate != null)
                    studentToUpdate.ClassId = classInfo.Id;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Delete(Guid Id, Guid? instituteId = null)
        {
            var query = _context.Classes.AsQueryable();
            if (instituteId.HasValue)
            {
                query = query.Where(c => c.InstituteId == instituteId.Value);
            }

            var classToDelete = await query.FirstOrDefaultAsync(c => c.Id == Id);
            if (classToDelete != null)
            {
                _context.Classes.Remove(classToDelete);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await _context
                .Classes.Include(c => c.StudyProgram)
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .ToListAsync();
        }

        public async Task<Class?> GetById(Guid id, Guid? instituteId = null)
        {
            var query = _context
                .Classes.IgnoreQueryFilters()
                .Include(c => c.StudyProgram)
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .ThenInclude(s => s.StudentInfo)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(c => c.InstituteId == instituteId.Value);
            }

            return await query.SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateAsync(Class newClass)
        {
            var existingClass = await _context
                .Classes.Include(c => c.Students)
                .Include(c => c.Teachers)
                .FirstOrDefaultAsync(c => c.Id == newClass.Id);

            if (existingClass == null)
                return false;

            newClass.CreatedAt = existingClass.CreatedAt;

            existingClass.Name = newClass.Name;
            existingClass.Gender = newClass.Gender;
            existingClass.ClassTime = newClass.ClassTime;
            existingClass.ClassDays = newClass.ClassDays;
            if (newClass.StudyProgramId.HasValue)
            {
                existingClass.StudyProgramId = newClass.StudyProgramId;
            }

            //clearing the ids of students before updating
            foreach (var student in existingClass.Students)
            {
                student.ClassId = null;
            }
            // Update relationships — clear and re-add
            existingClass.Students.Clear();
            foreach (var student in newClass.Students)
            {
                // Attach existing students from context (avoid re-creating)
                var trackedStudent = await _context.Students.FirstOrDefaultAsync(s =>
                    s.UserId == student.UserId
                );
                if (trackedStudent != null)
                {
                    existingClass.Students.Add(trackedStudent);
                    trackedStudent.ClassId = existingClass.Id;
                }
            }

            existingClass.Teachers.Clear();
            foreach (var teacher in newClass.Teachers)
            {
                var trackedTeacher = await _context.Teachers.FirstOrDefaultAsync(t =>
                    t.UserId == teacher.UserId
                );
                if (trackedTeacher != null)
                    existingClass.Teachers.Add(trackedTeacher);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Class>> GetAllByInstituteAsync(Guid instituteId)
        {
            return await _context
                .Classes.Include(c => c.StudyProgram)
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .Where(c => c.InstituteId == instituteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetAllByInstituteAndClassDaysAsync(
            Guid instituteId,
            ClassDaysEnum workingDays
        )
        {
            return await _context
                .Classes.Include(c => c.StudyProgram)
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .Where(c =>
                    c.InstituteId == instituteId && c.ClassDays.Any(day => day == workingDays)
                )
                .ToListAsync();
        }

        public async Task<bool> RestoreClassAsync(Guid classId, Guid? instituteId = null)
        {
            var query = _context.Classes.IgnoreQueryFilters().AsQueryable();
            if (instituteId.HasValue)
            {
                query = query.Where(c => c.InstituteId == instituteId.Value);
            }

            Class? foundClass = await query.FirstOrDefaultAsync(c => c.Id == classId);
            if (foundClass == null || !foundClass.IsDeleted)
                return false;
            foundClass.IsDeleted = false;
            foundClass.DeletedAt = null;
            foundClass.DeletedBy = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Class>> GetArchivedClassesByInstituteAsync(Guid instituteId)
        {
            return await _context.Classes
                .IgnoreQueryFilters()
                .Include(c => c.StudyProgram)
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .Where(c => c.IsDeleted && c.InstituteId == instituteId)
                .OrderByDescending(c => c.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetArchivedClassesAsync()
        {
            return await _context.Classes
                .IgnoreQueryFilters()
                .Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .Where(c => c.IsDeleted)
                .OrderByDescending(c => c.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null)
        {
            var query = _context.Classes
                .IgnoreQueryFilters()
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(c => c.InstituteId == instituteId.Value);
            }

            var activeCount = await query.CountAsync(c => !c.IsDeleted);
            var archivedCount = await query.CountAsync(c => c.IsDeleted);

            return (activeCount, archivedCount);
        }
    }
}

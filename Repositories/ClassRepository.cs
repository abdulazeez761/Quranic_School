using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Data;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Repositories
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

        public async Task<bool> Delete(Guid Id)
        {
            var classToDelete = await _context
                .Classes.Include(c => c.StudentAttendances)
                .Include(c => c.TeacherAttendance)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (classToDelete != null)
            {
                _context.StudentAttendances.RemoveRange(classToDelete.StudentAttendances);
                foreach (Student student in classToDelete.Students)
                {
                    student.ClassId = null;
                }

                _context.Classes.Remove(classToDelete);

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await _context
                .Classes.Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .ToListAsync();
        }

        public async Task<Class?> GetById(Guid id)
        {
            return await _context
                .Classes.Include(c => c.Teachers)
                .ThenInclude(t => t.TeacherInfo)
                .Include(c => c.Students)
                .ThenInclude(s => s.StudentInfo)
                .SingleOrDefaultAsync(c => c.Id == id);
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
    }
}

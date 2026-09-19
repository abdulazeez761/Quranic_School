using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user, Student ReceivedStudent)
        {
            ReceivedStudent.StudentInfo = user;
            await _context.Students.AddAsync(ReceivedStudent);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid? instituteId = null)
        {
            var query = _context.Students.Include(s => s.StudentInfo).AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            var student = await query.FirstOrDefaultAsync(s => s.UserId == id);

            if (student is null)
                return false;

            _context.Students.Remove(student);
            if (student.StudentInfo != null)
            {
                _context.Users.Remove(student.StudentInfo);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .FirstOrDefaultAsync(t => t.StudentInfo.Email == email);
        }

        public async Task<Student?> GetByIdAsync(Guid id, Guid? instituteId = null)
        {
            var query = _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.wirds)
                .Include(s => s.Classes)
                .ThenInclude(c => c.Teachers)
                .Include(s => s.Attendances)
                .ThenInclude(a => a.Class)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            return await query.FirstOrDefaultAsync(t => t.UserId == id);
        }

        public async Task<Student?> GetStudentBasicByIdAsync(Guid id, Guid? instituteId = null)
        {
            var query = _context.Students.Include(s => s.StudentInfo).AsNoTracking().AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            return await query.FirstOrDefaultAsync(s => s.UserId == id);
        }

        public async Task<Student?> GetStudentWithClassesAsync(Guid id, Guid? instituteId = null)
        {
            var query = _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .ThenInclude(c => c.Teachers)
                .AsNoTracking()
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            return await query.FirstOrDefaultAsync(t => t.UserId == id);
        }

        public async Task UpdateAsync(EditStudentDto student)
        {
            var existingStudent = await _context
                .Students.Include(s => s.StudentInfo)
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.UserId == (student.StudentID ?? Guid.Empty));
            if (existingStudent is null || student is null)
                return;
            existingStudent.StudentInfo.FirstName =
                student.FirstName ?? existingStudent.StudentInfo.FirstName;
            existingStudent.StudentInfo.SecondName =
                student.SecondName ?? existingStudent.StudentInfo.SecondName;
            existingStudent.StudentInfo.PhoneNumber =
                student.PhoneNumber ?? existingStudent.StudentInfo.PhoneNumber;
            existingStudent.StudentInfo.Username =
                student.Username ?? existingStudent.StudentInfo.Username;
            existingStudent.StudentInfo.Email = student.Email ?? existingStudent.StudentInfo.Email;
            existingStudent.StudentInfo.Password =
                student.Password ?? existingStudent.StudentInfo.Password;
            if (student.DateOfBirth == null)
                existingStudent.DateOfBirth = student.DateOfBirth;

            var studentInstituteId = existingStudent.StudentInfo.InstituteId;

            if (student.ClassId.HasValue && student.ClassId != Guid.Empty)
            {
                var legacyClass = await _context.Classes.FindAsync(student.ClassId.Value);
                if (legacyClass != null && (!studentInstituteId.HasValue || legacyClass.InstituteId == studentInstituteId.Value))
                {
                    existingStudent.ClassId = student.ClassId;
                }
            }

            if (student.ParentId.HasValue && student.ParentId != Guid.Empty)
            {
                var parent = await _context.Parents.Include(p => p.ParentInfo)
                    .FirstOrDefaultAsync(p => p.UserId == student.ParentId.Value);
                if (parent != null && (!studentInstituteId.HasValue || parent.ParentInfo?.InstituteId == studentInstituteId.Value))
                {
                    existingStudent.ParentId = student.ParentId;
                }
            }

            if (student.MemorizedJuz != null)
                existingStudent.MemorizedJuz = student.MemorizedJuz;

            existingStudent.TajwidLevel = student.TajwidLevel ?? existingStudent.TajwidLevel;

            if (student.sex != null)
                existingStudent.sex = student.sex;

            if (student.ClassesIds != null)
            {
                existingStudent.Classes.Clear();
                foreach (var classId in student.ClassesIds)
                {
                    var classEntity = await _context.Classes.FindAsync(classId);
                    if (classEntity != null && (!studentInstituteId.HasValue || classEntity.InstituteId == studentInstituteId.Value))
                    {
                        existingStudent.Classes.Add(classEntity);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> GetAllByInstituteAsync(Guid instituteId)
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Where(s => s.StudentInfo.InstituteId == instituteId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsWithWirdsAndAttendancesByInstituteAsync(
            Guid instituteId,
            Guid? classId = null,
            string? search = null
        )
        {
            var query = _context
                .Students.Include(s => s.StudentInfo)
                .Include(s => s.Classes)
                .Include(s => s.Attendances)
                .Include(s => s.wirds)
                .Where(s => s.StudentInfo.InstituteId == instituteId);

            if (classId.HasValue)
            {
                query = query.Where(s => s.Classes.Any(c => c.Id == classId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(s =>
                    s.StudentInfo.FirstName.ToLower().Contains(term)
                    || s.StudentInfo.SecondName.ToLower().Contains(term)
                    || (s.StudentInfo.FirstName + " " + s.StudentInfo.SecondName)
                        .ToLower()
                        .Contains(term)
                );
            }

            return await query.AsSplitQuery().AsNoTracking().ToListAsync();
        }

        public async Task ApplyProgressDeltaAsync(
            Guid studentId,
            decimal memorizedPagesDelta,
            decimal reviewedPagesDelta
        )
        {
            if (memorizedPagesDelta == 0 && reviewedPagesDelta == 0)
                return;

            await _context
                .Students.Where(s => s.UserId == studentId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(
                            s => s.MemorizedPages,
                            s => s.MemorizedPages + memorizedPagesDelta
                        )
                        .SetProperty(
                            s => s.ReviewedPages,
                            s => s.ReviewedPages + reviewedPagesDelta
                        )
                );
        }

        public async Task<IEnumerable<Student>> GetStudentByInstituteIdAsyncAndClassDay(
            Guid instituteId,
            DateTime selectedDate
        )
        {
            ClassDaysEnum dayOfWeek = (ClassDaysEnum)(int)(selectedDate.DayOfWeek + 1);
            bool isPastDate = selectedDate.Date < DateTime.Today;
            var query = _context.Students.AsQueryable();

            if (isPastDate)
            {
                query = query
                    .IgnoreQueryFilters()
                    .Where(s =>
                        !s.IsDeleted
                        || s.Attendances.Any(a => a.Date.Date == selectedDate.Date)
                        || s.wirds.Any(w => w.AssignedDate.Date == selectedDate.Date)
                    );
            }

            return await query
                .Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Include(s => s.Attendances.Where(a => a.Date.Date == selectedDate.Date))
                .Include(s => s.wirds.Where(w => w.AssignedDate.Date == selectedDate.Date))
                .Where(s =>
                    s.StudentInfo.InstituteId == instituteId
                    && (
                        s.Classes.Any(c => c.ClassDays.Any(day => day == dayOfWeek))
                        || s.Attendances.Any(a => a.Date.Date == selectedDate.Date)
                        || s.wirds.Any(w => w.AssignedDate.Date == selectedDate.Date)
                    )
                )
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassIdAsync(Guid classId)
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Include(s => s.wirds)
                .Where(s => s.Classes.Any(c => c.Id == classId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> RestoreStudentAsync(Guid studentId, Guid? instituteId = null)
        {
            var query = _context
                .Students.IgnoreQueryFilters()
                .Include(s => s.StudentInfo)
                .AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            var student = await query.FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null || !student.IsDeleted)
                return false;

            student.IsDeleted = false;
            student.DeletedAt = null;
            student.DeletedBy = null;
            if (student.StudentInfo != null)
            {
                student.StudentInfo.IsDeleted = false;
                student.StudentInfo.DeletedAt = null;
                student.StudentInfo.DeletedBy = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Student>> GetArchivedByInstituteAsync(Guid instituteId)
        {
            return await _context
                .Students.IgnoreQueryFilters()
                .Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Where(s => s.IsDeleted && s.StudentInfo.InstituteId == instituteId)
                .OrderByDescending(s => s.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetArchivedAsync()
        {
            return await _context
                .Students.IgnoreQueryFilters()
                .Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Where(s => s.IsDeleted)
                .OrderByDescending(s => s.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(int activeCount, int archivedCount)> GetCountsAsync(
            Guid? instituteId = null
        )
        {
            var query = _context.Students.IgnoreQueryFilters().AsQueryable();

            if (instituteId.HasValue)
            {
                query = query.Where(s => s.StudentInfo.InstituteId == instituteId.Value);
            }

            var activeCount = await query.CountAsync(s => !s.IsDeleted);
            var archivedCount = await query.CountAsync(s => s.IsDeleted);

            return (activeCount, archivedCount);
        }
    }
}

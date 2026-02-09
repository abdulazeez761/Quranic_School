using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Common.Security;
using Hifz.Data;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Repositories
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync(); //save user to get the generated Id

                ReceivedStudent.UserId = user.Id;

                await _context.Students.AddAsync(ReceivedStudent);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // ✅ Both succeeded, commit
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // ✅ Either failed, rollback everything
                throw new Exception(
                    ex.InnerException?.Message ?? "An error occurred while adding the student."
                );
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var student = await GetByIdAsync(id);

            if (student is null)
                return;
            _context.Students.Remove(student);
            _context.Users.Remove(student.StudentInfo);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.Classes)
                .Include(s => s.Attendances)
                .Include(s => s.wirds)
                .ToListAsync();
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .FirstOrDefaultAsync(t => t.StudentInfo.Email == email);
        }

        public async Task<Student?> GetByIdAsync(Guid id)
        {
            return await _context
                .Students.Include(t => t.StudentInfo)
                .Include(s => s.wirds)
                .Include(s => s.Classes)
                .Include(s => s.Attendances)
                .ThenInclude(a => a.Class)
                .FirstOrDefaultAsync(t => t.UserId == id);
        }

        public async Task UpdateAsync(EditStudentDto student)
        {
            var existingStudent = await _context.Students
                .Include(s => s.StudentInfo)
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
            existingStudent.StudentInfo.Username = student.Username ?? existingStudent.StudentInfo.Username;
            existingStudent.StudentInfo.Email = student.Email ?? existingStudent.StudentInfo.Email;
            existingStudent.StudentInfo.Password =
                student.Password ?? existingStudent.StudentInfo.Password;
            if (student.DateOfBirth == null)
                existingStudent.DateOfBirth = student.DateOfBirth;

            existingStudent.ClassId =
                student.ClassId == Guid.Empty ? existingStudent.ClassId : student.ClassId;

            existingStudent.ParentId =
                student.ParentId == Guid.Empty ? existingStudent.ParentId : student.ParentId;
            if (student.MemorizedJuz != null)
                existingStudent.MemorizedJuz = student.MemorizedJuz;
            else
                existingStudent.MemorizedJuz = existingStudent.MemorizedJuz;
            existingStudent.TajwidLevel = student.TajwidLevel ?? existingStudent.TajwidLevel;

            if (student.sex != null)
                existingStudent.sex = student.sex;

            if (student.ClassesIds != null)
            {
                existingStudent.Classes.Clear();
                foreach (var classId in student.ClassesIds)
                {
                    var classEntity = await _context.Classes.FindAsync(classId);
                    if (classEntity != null)
                    {
                        existingStudent.Classes.Add(classEntity);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Models;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Infrastructure.Repositories
{
    public class InstituteRepository : IInstituteRepository
    {
        private readonly ApplicationDbContext _context;

        public InstituteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Institute> CreateAsync(Institute institute)
        {
            if (institute == null)
                throw new ArgumentNullException(nameof(institute));
            try
            {
                await _context.Institutes.AddAsync(institute);
                await _context.SaveChangesAsync();
                return institute;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the institute: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var institute = await _context.Institutes.FindAsync(id);
            if (institute != null)
            {
                _context.Institutes.Remove(institute);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Institute institute)
        {
            var existing = await _context.Institutes.FindAsync(institute.Id);
            if (existing == null)
                throw new Exception("المركز غير موجود");

            existing.Name = institute.Name;
            existing.Description = institute.Description;
            existing.Address = institute.Address;
            existing.PhoneNumber = institute.PhoneNumber;
            existing.Logo = institute.Logo;
            existing.IsActive = institute.IsActive;
            existing.ManagerId = institute.ManagerId;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Institute>> GetAllAsync()
        {
            return await _context.Institutes.Include(i => i.Manager).ToListAsync();
        }

        public async Task<Institute> GetByIdAsync(Guid id)
        {
            return await _context
                .Institutes.Include(i => i.Manager)
                .Include(i => i.Users)
                .Include(i => i.Classes)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<User>> GetInstituteAdminsAsync(Guid instituteId)
        {
            return await _context
                .Users.Where(u => u.InstituteId == instituteId && u.Role == UserRole.Admin)
                .ToListAsync();
        }

        public async Task<int> GetInstituteClassCountAsync(Guid instituteId)
        {
            return await _context.Classes.CountAsync(c => c.InstituteId == instituteId);
        }

        public async Task<int> GetInstituteStudentCountAsync(Guid instituteId)
        {
            return await _context.Users.CountAsync(u =>
                u.InstituteId == instituteId && u.Role == UserRole.Student
            );
        }

        public async Task<int> GetInstituteTeacherCountAsync(Guid instituteId)
        {
            return await _context.Users.CountAsync(u =>
                u.InstituteId == instituteId && u.Role == UserRole.Teacher
            );
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Institutes.AnyAsync(i => i.Name == name);
        }
    }
}

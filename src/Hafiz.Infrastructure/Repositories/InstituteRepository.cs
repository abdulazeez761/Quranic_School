using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Models;

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
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.WriteLine($"An error occurred while creating the institute: {ex.Message}");
                return null;
            }
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Institute institute)
        {
            throw new NotImplementedException();
        }

        public Task<List<Institute>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Institute> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetInstituteAdminsAsync(Guid instituteId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetInstituteClassCountAsync(Guid instituteId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetInstituteStudentCountAsync(Guid instituteId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetInstituteTeacherCountAsync(Guid instituteId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> NameExistsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}

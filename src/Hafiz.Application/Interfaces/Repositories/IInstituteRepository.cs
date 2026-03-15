using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Models;

namespace Hafiz.Application.Interfaces.Repositories
{
    public interface IInstituteRepository
    {
        Task<Institute> GetByIdAsync(Guid id);
        Task<List<Institute>> GetAllAsync();
        Task<Institute> CreateAsync(Institute institute);
        Task UpdateAsync(Institute institute);
        Task DeleteAsync(Guid id);
        Task<List<User>> GetInstituteAdminsAsync(Guid instituteId);
        Task<bool> NameExistsAsync(string name);
        Task<int> GetInstituteStudentCountAsync(Guid instituteId);
        Task<int> GetInstituteTeacherCountAsync(Guid instituteId);
        Task<int> GetInstituteClassCountAsync(Guid instituteId);
    }
}

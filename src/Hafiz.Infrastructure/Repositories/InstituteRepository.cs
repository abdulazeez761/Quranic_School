using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Domain.Entities;
using Hafiz.Models;

namespace Hafiz.Infrastructure.Repositories
{
    public class InstituteRepository : IInstituteRepository
    {
        public Task<Institute> CreateAsync(Institute institute)
        {
            throw new NotImplementedException();
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

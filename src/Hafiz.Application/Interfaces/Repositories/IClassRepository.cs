using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<Class>> GetAllAsync();
        Task AddAsync(Class classInfo);
        Task<Class> GetById(Guid id);
        Task<bool> Delete(Guid Id);
        Task<bool> UpdateAsync(Class newClass);
    }
}

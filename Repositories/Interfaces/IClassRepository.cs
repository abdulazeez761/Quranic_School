using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
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

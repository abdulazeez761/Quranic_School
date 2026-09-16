using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;

namespace Hafiz.Repositories.Interfaces;

public interface IMatnRepository
{
    Task<Matn?> GetByIdAsync(Guid id);
    Task<IEnumerable<Matn>> GetAllAvailableAsync(Guid? instituteId = null, Hafiz.Domain.Enums.MatnCategory? category = null);
    Task<Matn> AddAsync(Matn matn);
    Task<bool> UpdateAsync(Matn matn);
    Task<bool> DeleteAsync(Guid id, Guid? instituteId = null);
    Task<bool> ExistsAsync(Guid id);
}

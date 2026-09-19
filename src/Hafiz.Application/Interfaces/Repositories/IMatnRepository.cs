using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface IMatnRepository
{
    Task<Matn?> GetByIdAsync(Guid id);
    Task<IEnumerable<Matn>> GetAllAvailableAsync(Guid? instituteId = null, MatnCategory? category = null);
    Task<IEnumerable<Matn>> GetByProgramIdAsync(Guid programId);
    Task<IEnumerable<Matn>> GetUnassignedLibraryMatnsAsync(Guid? instituteId = null);
    Task<Matn> AddAsync(Matn matn);
    Task<bool> UpdateAsync(Matn matn);
    Task<bool> DeleteAsync(Guid id, Guid? instituteId = null);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> HasHistoricalRecordsAsync(Guid id);
}

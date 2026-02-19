using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface IParentRepository
    {
        Task AddAsync(Parent parent);
        Task<IEnumerable<Parent>> GetAllAsync();
        Task<Parent?> GetByIdAsync(Guid id);
        Task<Parent?> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(EditParentDto dto);
        Task DeleteAsync(Guid id);
    }
}

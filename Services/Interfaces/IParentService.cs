using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Services.Interfaces
{
    public interface IParentService
    {
        Task<(bool Success, string ErrorMessage)> AddAsync(RegisterParentDto parent);
        Task<IEnumerable<Parent>> GetAllAsync();
        Task<ParentDto?> GetByIdAsync(Guid id);
        Task<Parent?> GetParentByUserIdAsync(Guid userId);
        Task<IEnumerable<Student>> GetParentChildrenAsync(Guid userId);
        Task UpdateAsync(EditParentDto parent);
        Task DeleteAsync(Guid id);
    }
}

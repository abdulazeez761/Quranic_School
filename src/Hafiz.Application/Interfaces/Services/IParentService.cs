using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IParentService
    {
        Task<(bool Success, string ErrorMessage)> AddAsync(RegisterParentDto parent, Guid? instituteId = null);
        Task<IEnumerable<Parent>> GetAllAsync();
        Task<IEnumerable<Parent>> GetAllByInstituteAsync(Guid instituteId);
        Task<ParentDto?> GetByIdAsync(Guid id);
        Task<Parent?> GetParentByUserIdAsync(Guid userId);
        Task<IEnumerable<Student>> GetParentChildrenAsync(Guid userId);
        Task UpdateAsync(EditParentDto parent);
        Task DeleteAsync(Guid id);
    }
}

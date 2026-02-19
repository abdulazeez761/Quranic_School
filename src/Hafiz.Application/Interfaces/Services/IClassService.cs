using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassDto>> GetClassesAsync();
        Task CreateAsync(CreateClassDto classDto);
        Task<IEnumerable<Class>> ViewClasses();
        Task<ClassDto> GetClassById(Guid id);
        Task<bool> DeleteClass(Guid Id);
        Task<bool> UpdateAsync(ClassDto classDto);
    }
}

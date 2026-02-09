using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Services.Interfaces
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

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
        Task<IEnumerable<Class>> ViewClasses();
        Task<ClassDto?> GetClassById(Guid id, Guid? instituteId = null);
        Task<bool> DeleteClass(Guid Id, Guid? instituteId = null);
        Task<bool> RestoreClassAsync(Guid classId, Guid? instituteId = null);
        Task<bool> UpdateAsync(ClassDto classDto);
        Task<List<ClassDto>> GetClassesByInstituteAsync(Guid instituteId);
        Task<IEnumerable<Class>> ViewClassesByInstitute(Guid instituteId);
        Task CreateAsync(CreateClassDto classDto, Guid? instituteId = null);
        Task<IEnumerable<Class>> ViewClassesByInstituteAndClassDaysAsync(
            Guid instituteId,
            DateTime workingDays
        );
    }
}

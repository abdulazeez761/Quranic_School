using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<Teacher>> GetAllTeachersAsync();
        Task<IEnumerable<Teacher>> GetAllTeachersByInstituteAsync(Guid instituteId);
        Task<bool> DeleteTeacherAsync(Guid teacherId, Guid? instituteId = null);
        Task<TeacherDto?> GetTeacherByIDAsync(Guid teacherId, Guid? instituteId = null);
        Task<bool> RestoreTeacherAsync(Guid teacherId, Guid? instituteId = null);
        Task UpdateTeacherAsync(TeacherDto teacher);
        Task<IList<Class>> GetTeacherClasses(Guid teacherId);
        Task<IEnumerable<Teacher>> GetArchivedTeachersByInstituteAsync(Guid instituteId);
        Task<IEnumerable<Teacher>> GetArchivedTeachersAsync();
        Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null);
    }
}

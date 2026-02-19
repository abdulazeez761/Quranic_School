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
        Task<bool> DeleteTeacherAsync(Guid teacherId);
        Task<TeacherDto?> GetTeacherByIDAsync(Guid teacherId);
        Task UpdateTeacherAsync(TeacherDto teacher);
        Task<IList<Class>> GetTeacherClasses(Guid teacherId);
    }
}

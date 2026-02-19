using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface ITeacherRepository
    {
        Task AddAsync(Teacher teacher);

        // Task<Teacher?> GetByIdAsync(int id);
        Task<IEnumerable<Teacher>> GetAllAsync();

        Task DeleteAsync(Teacher teacher);
        Task<Teacher?> GetTeacherByIDAsync(Guid teacherId);
        Task UpdateAsync(TeacherDto teacher);
        Task<IList<Class>> GetTeacherClasses(Guid teacherId);
    }
}

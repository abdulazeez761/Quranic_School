using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
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

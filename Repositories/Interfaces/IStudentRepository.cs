using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task AddAsync(User user, Student ReceivedStudent);
        Task<Student?> GetByIdAsync(Guid id);
        Task<Student?> GetByEmailAsync(string email);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(EditStudentDto student);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task AddAsync(User user, Student ReceivedStudent);
        Task<Student?> GetByIdAsync(Guid id);
        Task<Student?> GetByEmailAsync(string email);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(EditStudentDto student);
        Task<IEnumerable<Student>> GetAllByInstituteAsync(Guid instituteId);

        /// <summary>
        /// Atomically increments the student's cumulative memorized/reviewed page counters.
        /// Pass negative deltas to subtract (e.g. when a completed wird is deleted).
        /// </summary>
        Task ApplyProgressDeltaAsync(Guid studentId, decimal memorizedPagesDelta, decimal reviewedPagesDelta);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task AddAsync(User user, Student ReceivedStudent);
        Task<Student?> GetByIdAsync(Guid id, Guid? instituteId = null);
        Task<Student?> GetStudentBasicByIdAsync(Guid id, Guid? instituteId = null);
        Task<Student?> GetStudentWithClassesAsync(Guid id, Guid? instituteId = null);
        Task<Student?> GetByEmailAsync(string email);
        Task<bool> DeleteAsync(Guid id, Guid? instituteId = null);
        Task UpdateAsync(EditStudentDto student);
        Task<IEnumerable<Student>> GetAllByInstituteAsync(Guid instituteId);
        Task<IEnumerable<Student>> GetStudentsWithWirdsAndAttendancesByInstituteAsync(
            Guid instituteId,
            Guid? classId = null,
            string? search = null
        );

        /// <summary>
        /// Atomically increments the student's cumulative memorized/reviewed page counters.
        /// Pass negative deltas to subtract (e.g. when a completed wird is deleted).
        /// </summary>
        Task ApplyProgressDeltaAsync(
            Guid studentId,
            decimal memorizedPagesDelta,
            decimal reviewedPagesDelta
        );

        Task<IEnumerable<Student>> GetStudentByInstituteIdAsyncAndClassDay(
            Guid instituteId,
            DateTime selectedDate
        );

        Task<IEnumerable<Student>> GetStudentsByClassIdAsync(Guid classId);
        Task<bool> RestoreStudentAsync(Guid studentId, Guid? instituteId = null);
        Task<IEnumerable<Student>> GetArchivedByInstituteAsync(Guid instituteId);
        Task<IEnumerable<Student>> GetArchivedAsync();
        Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null);
    }
}

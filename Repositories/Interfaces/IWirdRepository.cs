using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
{
    public interface IWirdRepository
    {
        Task<bool> AddWirdAsync(WirdAssignment wird);
        Task<List<WirdAssignment>> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            DateTime from,
            DateTime to
        );
        Task<List<WirdAssignment>> GetWirdAssignmentsByStudentIdAsync(Guid studentID);
        Task<(
            List<WirdAssignment> wirds,
            int totalCount,
            int completedCount,
            int pendingCount
        )> GetWirdAssignmentsByStudentIdPaginatedAsync(
            Guid studentID,
            int pageNumber,
            int pageSize,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null
        );
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status);
        Task<WirdAssignment?> GetWirdByID(Guid Id);
        Task<bool> UpdateNote(Guid Id, string note);
        Task<bool> DeleteWirdAssignment(Guid id);
    }
}

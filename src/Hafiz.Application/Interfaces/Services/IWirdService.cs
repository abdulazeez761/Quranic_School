using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IWirdService
    {
        Task<(bool IsSuccess, string Message)> AddWirdAsync(WirdAssignment wird);
        Task<(bool IsSuccess, string Message)> UpdateWirdAsync(WirdAssignment wird);
        Task<List<WirdAssignment>?> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            string? fromDate,
            string? toDate
        );
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status);
        Task<bool> UpdateWirdNote(Guid Id, string Note);
        Task<bool> DeleteWirdAssignment(Guid id);
        Task<WirdAssignment?> GetWirdAssignmentByIdAsync(Guid id);
    }
}

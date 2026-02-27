using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class WirdService : IWirdService
    {
        private readonly IWirdRepository _wirdRepository;

        public WirdService(IWirdRepository wirdRepository)
        {
            _wirdRepository = wirdRepository;
        }

        public async Task<(bool IsSuccess, string Message)> AddWirdAsync(WirdAssignment wird)
        {
            if (wird.Status.ToString() != "notSet")
                wird.IsCompleted = true;
            bool isAdded = await _wirdRepository.AddWirdAsync(wird);
            return (
                isAdded,
                isAdded
                    ? "Wird has been successfully assigned!"
                    : "Failed to assign wird. No changes were made."
            );
        }

        public async Task<(bool IsSuccess, string Message)> UpdateWirdAsync(WirdAssignment wird)
        {
            bool isUpdated = await _wirdRepository.UpdateWirdAsync(wird);
            return (
                isUpdated,
                isUpdated
                    ? "Wird has been successfully updated!"
                    : "Failed to update wird. No changes were made."
            );
        }

        public async Task<bool> DeleteWirdAssignment(Guid id)
        {
            var Wird = await _wirdRepository.GetWirdByID(id);
            if (Wird == null)
                return false;
            bool isWirdDeleted = await _wirdRepository.DeleteWirdAssignment(id);
            return isWirdDeleted;
        }

        public async Task<WirdAssignment?> GetWirdAssignmentByIdAsync(Guid id)
        {
            return await _wirdRepository.GetWirdByID(id);
        }

        public async Task<List<WirdAssignment>?> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            string? fromDate,
            string? toDate
        )
        {
            DateTime from = string.IsNullOrEmpty(fromDate)
                ? DateTime.Today
                : DateTime.Parse(fromDate).Date;

            DateTime to = string.IsNullOrEmpty(toDate)
                ? DateTime.Today
                : DateTime.Parse(toDate).Date;

            return await _wirdRepository.GetWirdAssignmentsByClassIdAsync(classID, from, to);
        }

        public async Task<bool> UpdateStatus(Guid Id, AssignmentStatus status)
        {
            var Wird = await _wirdRepository.GetWirdByID(Id);
            if (Wird == null)
                return false;

            return await _wirdRepository.UpdateStatus(Id, status);
        }

        public async Task<bool> UpdateWirdNote(Guid Id, string Note)
        {
            var Wird = await _wirdRepository.GetWirdByID(Id);
            if (Wird == null)
                return false;

            return await _wirdRepository.UpdateNote(Id, Note);
        }
    }
}

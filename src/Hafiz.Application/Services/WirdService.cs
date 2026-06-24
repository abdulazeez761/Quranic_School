using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Common.Helper;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class WirdService : IWirdService
    {
        private readonly IWirdRepository _wirdRepository;
        private readonly IStudentRepository _studentRepository;

        public WirdService(IWirdRepository wirdRepository, IStudentRepository studentRepository)
        {
            _wirdRepository = wirdRepository;
            _studentRepository = studentRepository;
        }

        public async Task<(bool IsSuccess, string Message)> AddWirdAsync(WirdAssignment wird)
        {
            if (wird.Type == AssignmentType.Memorization)
            {
                var student = await _studentRepository.GetByIdAsync(wird.StudentId);
                if (student != null && WirdPageCalculator.IsHafiz(student))
                {
                    return (
                        false,
                        "تنبيه: الطالب قد أتم حفظ القرآن الكريم كاملاً. لا يمكن إضافة وِرد حفظ جديد."
                    );
                }
            }

            if (wird.Status.ToString() != "notSet")
                wird.IsCompleted = true;
            bool isAdded = await _wirdRepository.AddWirdAsync(wird);
            if (isAdded)
            {
                var (memDelta, revDelta) = ProgressContribution(wird);
                await _studentRepository.ApplyProgressDeltaAsync(wird.StudentId, memDelta, revDelta);
            }
            return (
                isAdded,
                isAdded
                    ? "Wird has been successfully assigned!"
                    : "Failed to assign wird. No changes were made."
            );
        }

        public async Task<(bool IsSuccess, string Message)> UpdateWirdAsync(WirdAssignment wird)
        {
            var existing = await _wirdRepository.GetWirdByID(wird.Id);
            var (oldMem, oldRev) = existing is null
                ? (0m, 0m)
                : ProgressContribution(existing);

            // Block memorization edits that would exceed the Quran for an already-hafiz student.
            // Compare against the student's total minus this wird's old memorization contribution
            // so editing a wird that's already counted doesn't falsely trip the limit.
            if (wird.Type == AssignmentType.Memorization)
            {
                var student = await _studentRepository.GetByIdAsync(wird.StudentId);
                if (student != null)
                {
                    decimal totalExcludingThis = WirdPageCalculator.TotalMemorizedPages(student) - oldMem;
                    if (totalExcludingThis >= WirdPageCalculator.TotalQuranPages)
                    {
                        return (
                            false,
                            "تنبيه: الطالب قد أتم حفظ القرآن الكريم كاملاً. لا يمكن إضافة وِرد حفظ جديد."
                        );
                    }
                }
            }

            bool isUpdated = await _wirdRepository.UpdateWirdAsync(wird);
            if (isUpdated)
            {
                var (newMem, newRev) = ProgressContribution(wird);
                await _studentRepository.ApplyProgressDeltaAsync(
                    wird.StudentId,
                    newMem - oldMem,
                    newRev - oldRev
                );
            }
            return (
                isUpdated,
                isUpdated
                    ? "Wird has been successfully updated!"
                    : "Failed to update wird. No changes were made."
            );
        }

        public async Task<bool> DeleteWirdAssignment(Guid id)
        {
            var wird = await _wirdRepository.GetWirdByID(id);
            if (wird == null)
                return false;
            var (memDelta, revDelta) = ProgressContribution(wird);
            bool isWirdDeleted = await _wirdRepository.DeleteWirdAssignment(id);
            if (isWirdDeleted && (memDelta != 0 || revDelta != 0))
            {
                await _studentRepository.ApplyProgressDeltaAsync(
                    wird.StudentId,
                    -memDelta,
                    -revDelta
                );
            }
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
            var wird = await _wirdRepository.GetWirdByID(Id);
            if (wird == null)
                return false;

            var (oldMem, oldRev) = ProgressContribution(wird);
            bool ok = await _wirdRepository.UpdateStatus(Id, status);
            if (ok)
            {
                wird.Status = status;
                var (newMem, newRev) = ProgressContribution(wird);
                await _studentRepository.ApplyProgressDeltaAsync(
                    wird.StudentId,
                    newMem - oldMem,
                    newRev - oldRev
                );
            }
            return ok;
        }

        public async Task<bool> UpdateWirdNote(Guid Id, string Note)
        {
            var Wird = await _wirdRepository.GetWirdByID(Id);
            if (Wird == null)
                return false;

            return await _wirdRepository.UpdateNote(Id, Note);
        }

        private static (decimal memorized, decimal reviewed) ProgressContribution(WirdAssignment wird)
        {
            decimal pages = WirdPageCalculator.ToPages(wird);
            if (pages <= 0 || wird.Status == AssignmentStatus.notSet)
                return (0m, 0m);

            return wird.Type switch
            {
                AssignmentType.Memorization => (pages, 0m),
                AssignmentType.Revision => (0m, pages),
                _ => (0m, 0m),
            };
        }
    }
}

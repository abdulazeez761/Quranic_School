using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface IMatnAssignmentRepository
{
    Task<MatnAssignment?> GetByIdAsync(Guid id);
    Task<IEnumerable<MatnAssignment>> GetByStudentAndClassAsync(Guid studentId, Guid classId);
    Task<IEnumerable<MatnAssignment>> GetByClassAndDateAsync(Guid classId, DateTime date);
    Task<IEnumerable<MatnAssignment>> GetByClassIdsAndDateAsync(IEnumerable<Guid> classIds, DateTime date);
    Task<IEnumerable<MatnAssignment>> GetByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<MatnAssignment>> GetByStudentIdAsync(Guid studentId);
    Task<MatnAssignment> AddAsync(MatnAssignment assignment);
    Task<bool> UpdateAsync(MatnAssignment assignment);
    Task<bool> DeleteAsync(Guid id);
    Task<(int memorizedCount, int revisedCount, int mudarasahCount)> GetCountsByClassAsync(Guid classId);
}

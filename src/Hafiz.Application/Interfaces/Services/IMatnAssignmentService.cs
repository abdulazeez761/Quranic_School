using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;

namespace Hafiz.Services.Interfaces;

public interface IMatnAssignmentService
{
    Task<MatnAssignmentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<MatnAssignmentDto>> GetByStudentAndClassAsync(Guid studentId, Guid classId);
    Task<IEnumerable<MatnAssignmentDto>> GetByClassAndDateAsync(Guid classId, DateTime date);
    Task<IEnumerable<MatnAssignmentDto>> GetByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<MatnAssignmentDto>> GetByStudentIdAsync(Guid studentId);
    Task<(bool Success, string Message, Guid? Id)> AssignAsync(AssignMatnDto dto, Guid teacherId);
    Task<(bool Success, string Message)> UpdateAsync(EditMatnAssignmentDto dto, Guid teacherId);
    Task<(bool Success, string Message)> UpdateStatusAsync(UpdateMatnStatusDto dto, Guid teacherId);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid teacherId);
}

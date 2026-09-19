using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface IStudentMatnProgressRepository
{
    Task<StudentMatnProgress?> GetByIdAsync(Guid id);
    Task<StudentMatnProgress?> GetByStudentAndMatnAsync(Guid studentId, Guid matnId);
    Task<IEnumerable<StudentMatnProgress>> GetByStudentAsync(Guid studentId);
    Task<IEnumerable<StudentMatnProgress>> GetByMatnAsync(Guid matnId);
    Task<IEnumerable<StudentMatnProgress>> GetByClassAsync(Guid classId);
    Task<StudentMatnProgress> AddAsync(StudentMatnProgress progress);
    Task<bool> UpdateAsync(StudentMatnProgress progress);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetCountByStatusAsync(Guid instituteId, StudyStatus? studyStatus = null, MemorizationStatus? memorizationStatus = null, ExamStatus? examStatus = null);
}

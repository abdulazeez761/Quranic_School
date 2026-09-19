using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;

namespace Hafiz.Services.Interfaces;

public interface IStudentMatnProgressService
{
    Task<StudentMatnProgressDto?> GetByIdAsync(Guid id);
    Task<StudentMatnProgressDto?> GetByStudentAndMatnAsync(Guid studentId, Guid matnId);
    Task<IEnumerable<StudentMatnProgressDto>> GetByStudentAsync(Guid studentId);
    Task<IEnumerable<StudentMatnProgressDto>> GetByClassAsync(Guid classId);
    Task<(bool Success, string Message, Guid? Id)> CreateOrUpdateAsync(CreateStudentMatnProgressDto dto, Guid teacherId);
    Task<(bool Success, string Message)> CompleteStudyAsync(Guid id, Guid teacherId);
    Task<(bool Success, string Message)> CompleteMemorizationAsync(Guid id, Guid teacherId);
    Task<(bool Success, string Message)> CompleteStudyAndMemorizationAsync(CompleteBothStudyAndMemorizationDto dto, Guid teacherId);
    Task<(bool Success, string Message)> RecordExamResultAsync(RecordExamResultDto dto, Guid teacherId);
    Task<(bool Success, string Message)> UpdateProgressAsync(UpdateStudentMatnProgressDto dto, Guid teacherId);
}

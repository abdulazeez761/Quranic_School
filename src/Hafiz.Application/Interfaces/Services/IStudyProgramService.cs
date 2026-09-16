using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.StudyProgram;

namespace Hafiz.Services.Interfaces;

public interface IStudyProgramService
{
    Task<StudyProgramDto?> GetByIdAsync(Guid id, Guid instituteId);
    Task<IEnumerable<StudyProgramDto>> GetAllByInstituteAsync(Guid instituteId, ProgramType? type = null);
    Task<IEnumerable<StudyProgramDto>> GetActiveByInstituteAsync(Guid instituteId, ProgramType? type = null);
    Task<(bool Success, string Message, Guid? Id)> CreateAsync(CreateStudyProgramDto dto, Guid instituteId);
    Task<(bool Success, string Message)> UpdateAsync(UpdateStudyProgramDto dto, Guid instituteId);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid instituteId);
    Task<StudyProgramDto?> GetOrCreateDefaultQuranProgramAsync(Guid instituteId);
}

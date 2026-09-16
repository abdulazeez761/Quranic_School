using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface IStudyProgramRepository
{
    Task<StudyProgram?> GetByIdAsync(Guid id, Guid? instituteId = null);
    Task<IEnumerable<StudyProgram>> GetAllByInstituteAsync(Guid instituteId, ProgramType? type = null);
    Task<IEnumerable<StudyProgram>> GetActiveByInstituteAsync(Guid instituteId, ProgramType? type = null);
    Task<StudyProgram> AddAsync(StudyProgram program);
    Task<bool> UpdateAsync(StudyProgram program);
    Task<bool> DeleteAsync(Guid id, Guid? instituteId = null);
    Task<bool> ExistsAsync(Guid id, Guid? instituteId = null);
    Task<StudyProgram?> GetDefaultQuranProgramAsync(Guid instituteId);
}

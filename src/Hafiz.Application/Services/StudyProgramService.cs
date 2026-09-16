using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.StudyProgram;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services;

public class StudyProgramService : IStudyProgramService
{
    private readonly IStudyProgramRepository _programRepository;
    private readonly IMatnRepository _matnRepository;

    public StudyProgramService(IStudyProgramRepository programRepository, IMatnRepository matnRepository)
    {
        _programRepository = programRepository;
        _matnRepository = matnRepository;
    }

    public async Task<StudyProgramDto?> GetByIdAsync(Guid id, Guid instituteId)
    {
        var program = await _programRepository.GetByIdAsync(id, instituteId);
        if (program == null)
            return null;

        return MapToDto(program);
    }

    public async Task<IEnumerable<StudyProgramDto>> GetAllByInstituteAsync(Guid instituteId, ProgramType? type = null)
    {
        var programs = await _programRepository.GetAllByInstituteAsync(instituteId, type);
        return programs.Select(MapToDto);
    }

    public async Task<IEnumerable<StudyProgramDto>> GetActiveByInstituteAsync(Guid instituteId, ProgramType? type = null)
    {
        var programs = await _programRepository.GetActiveByInstituteAsync(instituteId, type);
        return programs.Select(MapToDto);
    }

    public async Task<(bool Success, string Message, Guid? Id)> CreateAsync(CreateStudyProgramDto dto, Guid instituteId)
    {
        if (dto == null)
            return (false, "بيانات البرنامج غير صالحة.", null);

        if (dto.Type == ProgramType.Matn && dto.MatnId.HasValue)
        {
            var matnExists = await _matnRepository.ExistsAsync(dto.MatnId.Value);
            if (!matnExists)
                return (false, "المتن المحدد غير موجود في النظام.", null);
        }

        var program = new StudyProgram
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Type = dto.Type,
            MatnId = dto.Type == ProgramType.Matn ? dto.MatnId : null,
            InstituteId = instituteId,
            IsActive = true
        };

        var created = await _programRepository.AddAsync(program);
        return (true, "تم إنشاء البرنامج التعليمي بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateStudyProgramDto dto, Guid instituteId)
    {
        if (dto == null)
            return (false, "البيانات غير صالحة.");

        var existing = await _programRepository.GetByIdAsync(dto.Id, instituteId);
        if (existing == null)
            return (false, "البرنامج غير موجود أو غير مصرح بتعديله.");

        if (dto.Type == ProgramType.Matn && dto.MatnId.HasValue)
        {
            var matnExists = await _matnRepository.ExistsAsync(dto.MatnId.Value);
            if (!matnExists)
                return (false, "المتن المحدد غير موجود.");
        }

        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description?.Trim();
        existing.Type = dto.Type;
        existing.MatnId = dto.Type == ProgramType.Matn ? dto.MatnId : null;
        existing.IsActive = dto.IsActive;

        var updated = await _programRepository.UpdateAsync(existing);
        return updated ? (true, "تم تحديث البرنامج بنجاح.") : (false, "فشل حفظ التعديلات.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid instituteId)
    {
        var existing = await _programRepository.GetByIdAsync(id, instituteId);
        if (existing == null)
            return (false, "البرنامج غير موجود.");

        if (existing.Classes != null && existing.Classes.Any(c => !c.IsDeleted))
        {
            return (false, "لا يمكن حذف البرنامج لوجود حلقات فعالة مرتبطة به. يمكنك تعطيل البرنامج بدلاً من حذفه.");
        }

        var deleted = await _programRepository.DeleteAsync(id, instituteId);
        return deleted ? (true, "تم حذف البرنامج بنجاح.") : (false, "تعذر الحذف.");
    }

    public async Task<StudyProgramDto?> GetOrCreateDefaultQuranProgramAsync(Guid instituteId)
    {
        var defaultProg = await _programRepository.GetDefaultQuranProgramAsync(instituteId);
        if (defaultProg != null)
            return MapToDto(defaultProg);

        var (success, _, id) = await CreateAsync(new CreateStudyProgramDto
        {
            Name = "برنامج تحفيظ القرآن الكريم",
            Description = "البرنامج الافتراضي لحفظ ومراجعة القرآن الكريم",
            Type = ProgramType.Quran
        }, instituteId);

        if (success && id.HasValue)
        {
            var created = await _programRepository.GetByIdAsync(id.Value, instituteId);
            if (created != null)
                return MapToDto(created);
        }

        return null;
    }

    private static StudyProgramDto MapToDto(StudyProgram sp) => new()
    {
        Id = sp.Id,
        Name = sp.Name,
        Description = sp.Description,
        Type = sp.Type,
        InstituteId = sp.InstituteId,
        MatnId = sp.MatnId,
        MatnTitle = sp.Matn?.Title,
        IsActive = sp.IsActive,
        ClassesCount = sp.Classes?.Count(c => !c.IsDeleted) ?? 0,
        CreatedAt = sp.CreatedAt
    };
}

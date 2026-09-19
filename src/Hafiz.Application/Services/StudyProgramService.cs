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

        var program = new StudyProgram
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Type = dto.Type,
            InstituteId = instituteId,
            IsActive = true
        };

        var created = await _programRepository.AddAsync(program);

        // إذا كان البرنامج للمتون وتم تحديد متون مبدئية
        if (dto.Type == ProgramType.Matn && dto.InitialMatnIds != null && dto.InitialMatnIds.Any())
        {
            int order = 1;
            foreach (var matnId in dto.InitialMatnIds)
            {
                var matn = await _matnRepository.GetByIdAsync(matnId);
                if (matn != null && (matn.InstituteId == null || matn.InstituteId == instituteId))
                {
                    matn.StudyProgramId = created.Id;
                    matn.Order = order++;
                    await _matnRepository.UpdateAsync(matn);
                }
            }
        }

        return (true, "تم إنشاء البرنامج التعليمي بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateStudyProgramDto dto, Guid instituteId)
    {
        if (dto == null)
            return (false, "البيانات غير صالحة.");

        var existing = await _programRepository.GetByIdAsync(dto.Id, instituteId);
        if (existing == null)
            return (false, "البرنامج غير موجود أو غير مصرح بتعديله.");

        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description?.Trim();
        existing.Type = dto.Type;
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

        // إذا كان هناك متون تابعة، تحقق من سجلاتها التاريخية
        if (existing.Matuns != null && existing.Matuns.Any())
        {
            foreach (var m in existing.Matuns)
            {
                if (await _matnRepository.HasHistoricalRecordsAsync(m.Id))
                {
                    return (false, "لا يمكن حذف البرنامج لوجود متون تحتوي على سجلات إنجاز أو تسميع. يمكنك تعطيل البرنامج بدلاً من حذفه.");
                }
            }
            // فك ارتباط المتون
            foreach (var m in existing.Matuns)
            {
                m.StudyProgramId = null;
                await _matnRepository.UpdateAsync(m);
            }
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

    public async Task<(bool Success, string Message)> AddMatnToProgramAsync(Guid programId, Guid matnId, Guid instituteId)
    {
        var program = await _programRepository.GetByIdAsync(programId, instituteId);
        if (program == null)
            return (false, "البرنامج العلمي غير موجود أو غير مصرح به.");

        var matn = await _matnRepository.GetByIdAsync(matnId);
        if (matn == null)
            return (false, "المتن المحدد غير موجود.");

        // التحقق من صلاحية المعهد على المتن
        if (matn.InstituteId != null && matn.InstituteId != instituteId)
            return (false, "لا يمكن إضافة متن يتبع معهداً آخر.");

        if (matn.StudyProgramId == programId)
            return (false, "المتن مضاف إلى هذا البرنامج بالفعل.");

        // إذا كان المتن مرتبطاً ببرنامج آخر بالفعل
        if (matn.StudyProgramId.HasValue && matn.StudyProgramId != programId)
        {
            // إذا كان المتن خاصاً بالمعهد، نمنع ربطه بأكثر من برنامج لتجنب تداخل الترتيب
            return (false, "المتن مرتبط بالفعل ببرنامج تعليمي آخر.");
        }

        int nextOrder = program.Matuns.Any() ? program.Matuns.Max(m => m.Order) + 1 : 1;
        matn.StudyProgramId = programId;
        matn.Order = nextOrder;
        matn.IsActive = true;

        var updated = await _matnRepository.UpdateAsync(matn);
        return updated ? (true, "تمت إضافة المتن إلى البرنامج بنجاح.") : (false, "فشل حفظ التعديلات.");
    }

    public async Task<(bool Success, string Message)> RemoveMatnFromProgramAsync(Guid programId, Guid matnId, Guid instituteId)
    {
        var program = await _programRepository.GetByIdAsync(programId, instituteId);
        if (program == null)
            return (false, "البرنامج العلمي غير موجود.");

        var matn = await _matnRepository.GetByIdAsync(matnId);
        if (matn == null || matn.StudyProgramId != programId)
            return (false, "المتن غير مرتبط بهذا البرنامج.");

        // فك الارتباط بدلاً من الحذف الفيزيائي
        matn.StudyProgramId = null;
        matn.Order = 1;

        var updated = await _matnRepository.UpdateAsync(matn);
        return updated ? (true, "تمت إزالة المتن من البرنامج بنجاح.") : (false, "فشل حفظ التعديلات.");
    }

    public async Task<(bool Success, string Message)> ReorderMatunsAsync(Guid programId, List<Guid> orderedMatnIds, Guid instituteId)
    {
        var program = await _programRepository.GetByIdAsync(programId, instituteId);
        if (program == null)
            return (false, "البرنامج العلمي غير موجود.");

        for (int i = 0; i < orderedMatnIds.Count; i++)
        {
            var matnId = orderedMatnIds[i];
            var matn = program.Matuns.FirstOrDefault(m => m.Id == matnId);
            if (matn != null)
            {
                matn.Order = i + 1;
                await _matnRepository.UpdateAsync(matn);
            }
        }

        return (true, "تم إعادة ترتيب المتون بنجاح.");
    }

    public async Task<(bool Success, string Message)> ToggleMatnActiveAsync(Guid programId, Guid matnId, Guid instituteId)
    {
        var program = await _programRepository.GetByIdAsync(programId, instituteId);
        if (program == null)
            return (false, "البرنامج العلمي غير موجود.");

        var matn = program.Matuns.FirstOrDefault(m => m.Id == matnId);
        if (matn == null)
            return (false, "المتن غير موجود في هذا البرنامج.");

        matn.IsActive = !matn.IsActive;
        var updated = await _matnRepository.UpdateAsync(matn);
        return updated ? (true, matn.IsActive ? "تم تفعيل المتن." : "تم تعطيل المتن.") : (false, "فشل حفظ التعديل.");
    }

    private static StudyProgramDto MapToDto(StudyProgram sp) => new()
    {
        Id = sp.Id,
        Name = sp.Name,
        Description = sp.Description,
        Type = sp.Type,
        InstituteId = sp.InstituteId,
        IsActive = sp.IsActive,
        ClassesCount = sp.Classes?.Count(c => !c.IsDeleted) ?? 0,
        Matuns = sp.Matuns?.OrderBy(m => m.Order).Select(m => new MatnSummaryDto
        {
            Id = m.Id,
            Title = m.Title,
            Author = m.Author,
            Category = m.Category,
            TotalVerses = m.TotalVerses,
            TotalChapters = m.TotalChapters,
            DefaultUnit = m.DefaultUnit,
            Order = m.Order,
            PassingGrade = m.PassingGrade,
            IsActive = m.IsActive,
            InstituteId = m.InstituteId,
            StudyProgramId = m.StudyProgramId
        }).ToList() ?? new(),
        CreatedAt = sp.CreatedAt
    };
}

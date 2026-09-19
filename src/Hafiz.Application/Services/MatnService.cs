using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.Matn;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services;

public class MatnService : IMatnService
{
    private readonly IMatnRepository _matnRepository;

    public MatnService(IMatnRepository matnRepository)
    {
        _matnRepository = matnRepository;
    }

    public async Task<MatnDto?> GetByIdAsync(Guid id)
    {
        var matn = await _matnRepository.GetByIdAsync(id);
        return matn == null ? null : MapToDto(matn);
    }

    public async Task<IEnumerable<MatnDto>> GetAllAvailableAsync(Guid? instituteId = null, MatnCategory? category = null)
    {
        var matns = await _matnRepository.GetAllAvailableAsync(instituteId, category);
        return matns.Select(MapToDto);
    }

    public async Task<IEnumerable<MatnDto>> GetByProgramIdAsync(Guid programId)
    {
        var matns = await _matnRepository.GetByProgramIdAsync(programId);
        return matns.Select(MapToDto);
    }

    public async Task<IEnumerable<MatnDto>> GetUnassignedLibraryMatnsAsync(Guid? instituteId = null)
    {
        var matns = await _matnRepository.GetUnassignedLibraryMatnsAsync(instituteId);
        return matns.Select(MapToDto);
    }

    public async Task<(bool Success, string Message, Guid? Id)> CreateAsync(CreateMatnDto dto, Guid? instituteId = null)
    {
        if (dto == null)
            return (false, "بيانات المتن غير صالحة.", null);

        var matn = new Matn
        {
            Title = dto.Title.Trim(),
            Author = dto.Author?.Trim(),
            Category = dto.Category,
            TotalVerses = dto.TotalVerses,
            TotalChapters = dto.TotalChapters,
            DefaultUnit = dto.DefaultUnit,
            StudyProgramId = dto.StudyProgramId,
            Order = dto.Order > 0 ? dto.Order : 1,
            IsActive = dto.IsActive,
            InstituteId = instituteId // null إذا كان متناً عاماً، أو يحمل معرف المعهد إذا أضافه المعهد
        };

        var created = await _matnRepository.AddAsync(matn);
        return (true, "تمت إضافة المتن بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateMatnDto dto, Guid? instituteId = null)
    {
        var existing = await _matnRepository.GetByIdAsync(id);
        if (existing == null)
            return (false, "المتن غير موجود.");

        // لا يحق للمعهد تعديل متن عام للنظام
        if (instituteId.HasValue && existing.InstituteId != null && existing.InstituteId != instituteId.Value)
            return (false, "لا تملك صلاحية تعديل هذا المتن العام.");

        existing.Title = dto.Title.Trim();
        existing.Author = dto.Author?.Trim();
        existing.Category = dto.Category;
        existing.TotalVerses = dto.TotalVerses;
        existing.TotalChapters = dto.TotalChapters;
        existing.DefaultUnit = dto.DefaultUnit;
        if (dto.StudyProgramId.HasValue)
        {
            existing.StudyProgramId = dto.StudyProgramId;
        }
        existing.Order = dto.Order;
        existing.IsActive = dto.IsActive;

        var updated = await _matnRepository.UpdateAsync(existing);
        return updated ? (true, "تم تحديث بيانات المتن بنجاح.") : (false, "فشل التحديث.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid? instituteId = null)
    {
        var existing = await _matnRepository.GetByIdAsync(id);
        if (existing == null)
            return (false, "المتن غير موجود.");

        if (instituteId.HasValue && existing.InstituteId != null && existing.InstituteId != instituteId.Value)
            return (false, "لا تملك صلاحية حذف هذا المتن.");

        if (await _matnRepository.HasHistoricalRecordsAsync(id))
        {
            return (false, "لا يمكن حذف المتن لوجود سجلات إنجاز أو تسميع مرتبطة به. يمكنك إلغاء تفعيله بدلاً من حذفه.");
        }

        var deleted = await _matnRepository.DeleteAsync(id, instituteId);
        return deleted ? (true, "تم حذف المتن بنجاح.") : (false, "تعذر الحذف أو أن المتن عام للنظام.");
    }

    private static MatnDto MapToDto(Matn m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Author = m.Author,
        Category = m.Category,
        TotalVerses = m.TotalVerses,
        TotalChapters = m.TotalChapters,
        DefaultUnit = m.DefaultUnit,
        InstituteId = m.InstituteId,
        StudyProgramId = m.StudyProgramId,
        StudyProgramName = m.StudyProgram?.Name,
        Order = m.Order,
        IsActive = m.IsActive
    };
}

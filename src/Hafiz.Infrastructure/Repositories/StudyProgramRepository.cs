using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories;

public class StudyProgramRepository : IStudyProgramRepository
{
    private readonly ApplicationDbContext _context;

    public StudyProgramRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudyProgram?> GetByIdAsync(Guid id, Guid? instituteId = null)
    {
        var query = _context.StudyPrograms
            .Include(sp => sp.Matn)
            .Include(sp => sp.Classes)
            .AsQueryable();

        if (instituteId.HasValue)
        {
            query = query.Where(sp => sp.InstituteId == instituteId.Value);
        }

        return await query.FirstOrDefaultAsync(sp => sp.Id == id);
    }

    public async Task<IEnumerable<StudyProgram>> GetAllByInstituteAsync(Guid instituteId, ProgramType? type = null)
    {
        var query = _context.StudyPrograms
            .Include(sp => sp.Matn)
            .Include(sp => sp.Classes)
            .Where(sp => sp.InstituteId == instituteId);

        if (type.HasValue)
        {
            query = query.Where(sp => sp.Type == type.Value);
        }

        return await query.OrderByDescending(sp => sp.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<StudyProgram>> GetActiveByInstituteAsync(Guid instituteId, ProgramType? type = null)
    {
        var query = _context.StudyPrograms
            .Include(sp => sp.Matn)
            .Where(sp => sp.InstituteId == instituteId && sp.IsActive);

        if (type.HasValue)
        {
            query = query.Where(sp => sp.Type == type.Value);
        }

        return await query.OrderBy(sp => sp.Name).ToListAsync();
    }

    public async Task<StudyProgram> AddAsync(StudyProgram program)
    {
        await _context.StudyPrograms.AddAsync(program);
        await _context.SaveChangesAsync();
        return program;
    }

    public async Task<bool> UpdateAsync(StudyProgram program)
    {
        _context.StudyPrograms.Update(program);
        var affected = await _context.SaveChangesAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? instituteId = null)
    {
        var program = await GetByIdAsync(id, instituteId);
        if (program == null)
            return false;

        _context.StudyPrograms.Remove(program);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid? instituteId = null)
    {
        var query = _context.StudyPrograms.AsQueryable();
        if (instituteId.HasValue)
        {
            query = query.Where(sp => sp.InstituteId == instituteId.Value);
        }
        return await query.AnyAsync(sp => sp.Id == id);
    }

    public async Task<StudyProgram?> GetDefaultQuranProgramAsync(Guid instituteId)
    {
        var prog = await _context.StudyPrograms
            .FirstOrDefaultAsync(sp => sp.InstituteId == instituteId && sp.Type == ProgramType.Quran && sp.IsActive);

        if (prog == null)
        {
            prog = new StudyProgram
            {
                Id = Guid.NewGuid(),
                InstituteId = instituteId,
                Name = "برنامج القرآن الكريم",
                Type = ProgramType.Quran,
                Description = "برنامج الحفظ والمراجعة القرآني الأساسي",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _context.StudyPrograms.AddAsync(prog);
            await _context.SaveChangesAsync();
        }

        return prog;
    }
}

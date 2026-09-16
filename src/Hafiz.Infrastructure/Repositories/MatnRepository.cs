using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories;

public class MatnRepository : IMatnRepository
{
    private readonly ApplicationDbContext _context;

    public MatnRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Matn?> GetByIdAsync(Guid id)
    {
        return await _context.Matns.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Matn>> GetAllAvailableAsync(Guid? instituteId = null, Hafiz.Domain.Enums.MatnCategory? category = null)
    {
        // نرجع المتون العامة (InstituteId == null) بالإضافة للمتون الخاصة بالمعهد (إن وجد)
        var query = _context.Matns.AsQueryable();

        if (instituteId.HasValue)
        {
            query = query.Where(m => m.InstituteId == null || m.InstituteId == instituteId.Value);
        }
        else
        {
            query = query.Where(m => m.InstituteId == null);
        }

        if (category.HasValue)
        {
            query = query.Where(m => m.Category == category.Value);
        }

        return await query.OrderBy(m => m.Title).ToListAsync();
    }

    public async Task<Matn> AddAsync(Matn matn)
    {
        await _context.Matns.AddAsync(matn);
        await _context.SaveChangesAsync();
        return matn;
    }

    public async Task<bool> UpdateAsync(Matn matn)
    {
        _context.Matns.Update(matn);
        var affected = await _context.SaveChangesAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? instituteId = null)
    {
        var matn = await _context.Matns.FirstOrDefaultAsync(m => m.Id == id);
        if (matn == null)
            return false;

        // لا يمكن حذف متن عام من قبل معهد خاص
        if (instituteId.HasValue && matn.InstituteId != instituteId.Value)
            return false;

        _context.Matns.Remove(matn);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Matns.AnyAsync(m => m.Id == id);
    }
}

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

public class MatnAssignmentRepository : IMatnAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public MatnAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MatnAssignment?> GetByIdAsync(Guid id)
    {
        return await _context.MatnAssignments
            .Include(ma => ma.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(ma => ma.Class)
            .FirstOrDefaultAsync(ma => ma.Id == id);
    }

    public async Task<IEnumerable<MatnAssignment>> GetByStudentAndClassAsync(Guid studentId, Guid classId)
    {
        return await _context.MatnAssignments
            .Where(ma => ma.StudentId == studentId && ma.ClassId == classId)
            .OrderByDescending(ma => ma.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MatnAssignment>> GetByClassAndDateAsync(Guid classId, DateTime date)
    {
        var targetDate = date.Date;
        return await _context.MatnAssignments
            .Include(ma => ma.Student)
                .ThenInclude(s => s.StudentInfo)
            .Where(ma => ma.ClassId == classId && ma.AssignedDate.Date == targetDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MatnAssignment>> GetByClassIdsAndDateAsync(IEnumerable<Guid> classIds, DateTime date)
    {
        var targetDate = date.Date;
        var idsList = classIds.ToList();
        return await _context.MatnAssignments
            .Include(ma => ma.Student)
                .ThenInclude(s => s.StudentInfo)
            .Where(ma => idsList.Contains(ma.ClassId) && ma.AssignedDate.Date == targetDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MatnAssignment>> GetByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.MatnAssignments
            .Include(ma => ma.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(ma => ma.Class)
            .Where(ma => ma.ClassId == classId);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            query = query.Where(ma => ma.AssignedDate >= from);
        }

        if (toDate.HasValue)
        {
            var toExclusive = toDate.Value.Date.AddDays(1);
            query = query.Where(ma => ma.AssignedDate < toExclusive);
        }

        return await query.OrderByDescending(ma => ma.AssignedDate).ToListAsync();
    }

    public async Task<IEnumerable<MatnAssignment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.MatnAssignments
            .Include(ma => ma.Class)
            .Where(ma => ma.StudentId == studentId)
            .OrderByDescending(ma => ma.AssignedDate)
            .ToListAsync();
    }

    public async Task<MatnAssignment> AddAsync(MatnAssignment assignment)
    {
        await _context.MatnAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> UpdateAsync(MatnAssignment assignment)
    {
        _context.MatnAssignments.Update(assignment);
        var affected = await _context.SaveChangesAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await _context.MatnAssignments.FindAsync(id);
        if (assignment == null)
            return false;

        _context.MatnAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(int memorizedCount, int revisedCount, int mudarasahCount)> GetCountsByClassAsync(Guid classId)
    {
        var assignments = await _context.MatnAssignments
            .Where(ma => ma.ClassId == classId && ma.IsCompleted)
            .Select(ma => ma.PerformanceType)
            .ToListAsync();

        return (
            assignments.Count(t => t == MatnPerformanceType.Memorization),
            assignments.Count(t => t == MatnPerformanceType.Revision),
            assignments.Count(t => t == MatnPerformanceType.Mudarasah)
        );
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Data;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Infrastructure.Repositories
{
    public class StudentRoutinePlanRepository : IStudentRoutinePlanRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRoutinePlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentRoutinePlan?> GetPlanByStudentIdAsync(Guid studentId)
        {
            return await _context.StudentRoutinePlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId);
        }

        public async Task<bool> ExistsByStudentIdAsync(Guid studentId)
        {
            return await _context.StudentRoutinePlans
                .AnyAsync(p => p.StudentId == studentId);
        }

        public async Task AddAsync(StudentRoutinePlan plan)
        {
            await _context.StudentRoutinePlans.AddAsync(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudentRoutinePlan plan)
        {
            _context.StudentRoutinePlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentRoutinePlan plan)
        {
            _context.StudentRoutinePlans.Remove(plan);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StudentRoutinePlan>> GetPlansByClassIdAsync(Guid classId)
        {
            return await _context.StudentRoutinePlans
                .AsNoTracking()
                .Where(p => p.ClassId == classId)
                .ToListAsync();
        }
    }
}

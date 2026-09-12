using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hafiz.Application.Interfaces.Repositories
{
    public interface IStudentRoutinePlanRepository
    {
        // 1. الاستعلام الفردي
        Task<StudentRoutinePlan?> GetPlanByStudentIdAsync(Guid studentId);

        Task<bool> ExistsByStudentIdAsync(Guid studentId);

        // 3. الإضافة والتعديل
        Task AddAsync(StudentRoutinePlan plan);
        Task UpdateAsync(StudentRoutinePlan plan);

        Task DeleteAsync(StudentRoutinePlan plan);

        Task<List<StudentRoutinePlan>> GetPlansByClassIdAsync(Guid classId);
    }
}

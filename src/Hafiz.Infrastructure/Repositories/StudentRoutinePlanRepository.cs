using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;

namespace Hafiz.Infrastructure.Repositories
{
    public class StudentRoutinePlanRepository : IStudentRoutinePlanRepository
    {
        public Task AddAsync(StudentRoutinePlan plan)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(StudentRoutinePlan plan)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByStudentIdAsync(Guid studentId)
        {
            throw new NotImplementedException();
        }

        public Task<StudentRoutinePlan?> GetPlanByStudentIdAsync(Guid studentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentRoutinePlan>> GetPlansByClassIdAsync(Guid classId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(StudentRoutinePlan plan)
        {
            throw new NotImplementedException();
        }
    }
}

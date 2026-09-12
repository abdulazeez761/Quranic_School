using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.DTOs.StudentPlan;
using Hafiz.Services.Interfaces;

namespace Hafiz.Application.Services
{
    public class StudentRoutinePlanService : IStudentRoutinePlanService
    {
        private readonly IStudentRoutinePlanRepository _studentRoutinePlanRepository;

        public StudentRoutinePlanService(IStudentRoutinePlanRepository studentRoutinePlanRepository)
        {
            _studentRoutinePlanRepository = studentRoutinePlanRepository;
        }

        public Task<(bool Success, string Message)> AddPlanAsync(
            UpsertStudentRoutinePlanDto planDto
        )
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> DeletePlanAsync(Guid studentId)
        {
            throw new NotImplementedException();
        }

        public Task<StudentRoutinePlanDto?> GetPlanByStudentIdAsync(Guid studentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentRoutinePlanDto>> GetPlansByClassIdAsync(Guid classId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> SetPlanAsync(
            UpsertStudentRoutinePlanDto planDto
        )
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> UpdatePlanAsync(
            UpdateStudentRoutinePlanDto planDto
        )
        {
            throw new NotImplementedException();
        }
    }
}

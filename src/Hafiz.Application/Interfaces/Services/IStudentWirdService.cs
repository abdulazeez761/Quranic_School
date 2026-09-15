using System;
using System.Threading.Tasks;
using Hafiz.Application.DTO.StudentWird;

namespace Hafiz.Services.Interfaces
{
    public interface IStudentWirdService
    {
        /// <summary>
        /// Retrieves the comprehensive Wird context for a student including summary info,
        /// approved routine plan, latest assignments per category, and today's assignments.
        /// </summary>
        Task<StudentWirdContextDto?> GetStudentWirdContextAsync(Guid studentId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Services.Interfaces
{
    public interface IStudentService
    {
        public Task<IEnumerable<Student>> GetAllAsync();
        public Task<(bool Success, string ErrorMessage)> AddAsync(RegisterStudentDto student);
        public Task DeleteAsync(Guid id);
        public Task<StudentDto?> GetByIdAsync(Guid id);

        public Task UpdateAsync(EditStudentDto student);
        public Task<IEnumerable<Student>> GetStudentsByClassID(Guid? classID);

        // Student Portal Methods
        public Task<Student?> GetStudentByUserIdAsync(Guid userId);
        public Task<Student?> GetStudentByIdAsync(Guid studentId);
        public Task<IEnumerable<StudentAttendance>> GetStudentAttendanceAsync(Guid studentId);
        public Task<IEnumerable<WirdAssignment>> GetStudentWirdsAsync(Guid studentId);
        public Task<Hifz.DTOs.Wird.PaginatedWirdsResponse> GetStudentWirdsPaginatedAsync(
            Guid studentId,
            int pageNumber = 1,
            int pageSize = 5,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null
        );
    }
}

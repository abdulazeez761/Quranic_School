using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
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
        public Task<Hafiz.DTOs.Wird.PaginatedWirdsResponse> GetStudentWirdsPaginatedAsync(
            Guid studentId,
            int pageNumber = 1,
            int pageSize = 5,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null
        );
    }
}

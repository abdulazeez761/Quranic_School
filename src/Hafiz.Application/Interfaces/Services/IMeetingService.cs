using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IMeetingService
    {
        /// <summary>
        /// Starts a meeting for the specified class
        /// </summary>
        Task<(bool Success, string Message)> StartMeetingAsync(Guid classId, Guid teacherId);

        /// <summary>
        /// Ends an active meeting for the specified class
        /// </summary>
        Task<(bool Success, string Message)> EndMeetingAsync(Guid classId, Guid teacherId);

        /// <summary>
        /// Gets all active meetings for a teacher
        /// </summary>
        Task<IEnumerable<Class>> GetActiveMeetingsAsync(Guid teacherId);

        /// <summary>
        /// Gets all active meetings for a student's enrolled classes
        /// </summary>
        Task<IEnumerable<Class>> GetStudentActiveMeetingsAsync(Guid studentId);

        /// <summary>
        /// Checks if a user can join a specific meeting
        /// </summary>
        Task<(bool CanJoin, string Reason, string DisplayName, string Role)> CanJoinMeetingAsync(
            Guid userId,
            Guid classId,
            string userRole
        );

        /// <summary>
        /// Validates if a teacher owns a specific class
        /// </summary>
        Task<bool> IsTeacherOfClassAsync(Guid teacherId, Guid classId);

        /// <summary>
        /// Validates if a student is enrolled in a specific class
        /// </summary>
        Task<bool> IsStudentOfClassAsync(Guid studentId, Guid classId);

        /// <summary>
        /// Gets all active meetings for a parent's children
        /// </summary>
        Task<IEnumerable<Class>> GetParentActiveMeetingsAsync(Guid parentId);

        /// <summary>
        /// Validates if a parent owns a specific student
        /// </summary>
        Task<bool> IsParentOfStudentAsync(Guid parentId, Guid studentId);

        /// <summary>
        /// Gets a student by ID with related data
        /// </summary>
        Task<Student> GetStudentDetailsAsync(Guid studentId);

        /// <summary>
        /// Gets a class by ID with related data
        /// </summary>
        Task<Class> GetClassWithDetailsAsync(Guid classId);
    }
}

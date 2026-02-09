using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Data;
using Hifz.Models;
using Hifz.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hifz.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MeetingService> _logger;

        public MeetingService(ApplicationDbContext context, ILogger<MeetingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> StartMeetingAsync(
            Guid classId,
            Guid teacherId
        )
        {
            try
            {
                // Verify teacher owns this class
                if (!await IsTeacherOfClassAsync(teacherId, classId))
                {
                    return (false, "You are not authorized to start this meeting.");
                }

                var cls = await _context.Classes.FindAsync(classId);
                if (cls == null)
                {
                    return (false, "Class not found.");
                }

                if (cls.IsMeetingActive)
                {
                    return (false, "Meeting is already active.");
                }

                cls.IsMeetingActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Meeting started for class {ClassId} by teacher {TeacherId}",
                    classId,
                    teacherId
                );

                return (true, "Meeting started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting meeting for class {ClassId}", classId);
                return (false, "An error occurred while starting the meeting.");
            }
        }

        public async Task<(bool Success, string Message)> EndMeetingAsync(
            Guid classId,
            Guid teacherId
        )
        {
            try
            {
                // Verify teacher owns this class
                if (!await IsTeacherOfClassAsync(teacherId, classId))
                {
                    return (false, "You are not authorized to end this meeting.");
                }

                var cls = await _context.Classes.FindAsync(classId);
                if (cls == null)
                {
                    return (false, "Class not found.");
                }

                if (!cls.IsMeetingActive)
                {
                    return (false, "Meeting is not active.");
                }

                cls.IsMeetingActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Meeting ended for class {ClassId} by teacher {TeacherId}",
                    classId,
                    teacherId
                );

                return (true, "Meeting ended successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending meeting for class {ClassId}", classId);
                return (false, "An error occurred while ending the meeting.");
            }
        }

        public async Task<IEnumerable<Class>> GetActiveMeetingsAsync(Guid teacherId)
        {
            try
            {
                var teacher = await _context
                    .Teachers.Include(t => t.Classes)
                    .ThenInclude(c => c.Students)
                    .FirstOrDefaultAsync(t => t.UserId == teacherId);

                if (teacher == null)
                {
                    return new List<Class>();
                }

                // Filter active meetings in memory
                return teacher.Classes.Where(c => c.IsMeetingActive).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting active meetings for teacher {TeacherId}",
                    teacherId
                );
                return new List<Class>();
            }
        }

        public async Task<IEnumerable<Class>> GetStudentActiveMeetingsAsync(Guid studentId)
        {
            try
            {
                var student = await _context
                    .Students.Include(s => s.Classes)
                    .ThenInclude(c => c.Teachers)
                    .FirstOrDefaultAsync(s => s.UserId == studentId);

                if (student == null)
                {
                    return new List<Class>();
                }

                // Filter active meetings in memory
                return student.Classes.Where(c => c.IsMeetingActive).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting active meetings for student {StudentId}",
                    studentId
                );
                return new List<Class>();
            }
        }

        public async Task<(
            bool CanJoin,
            string Reason,
            string DisplayName,
            string Role
        )> CanJoinMeetingAsync(Guid userId, Guid classId, string userRole)
        {
            try
            {
                var cls = await GetClassWithDetailsAsync(classId);
                if (cls == null)
                {
                    return (false, "Class not found.", null, null);
                }

                if (!cls.IsMeetingActive)
                {
                    return (
                        false,
                        "This meeting is not currently active. Please wait for the teacher to start it.",
                        null,
                        null
                    );
                }

                // Check Teacher membership
                if (userRole == "Teacher")
                {
                    var teacher = await _context
                        .Teachers.Include(t => t.TeacherInfo)
                        .Include(t => t.Classes)
                        .FirstOrDefaultAsync(t => t.UserId == userId);

                    if (teacher != null && teacher.Classes.Any(c => c.Id == classId))
                    {
                        var displayName =
                            $"{teacher.TeacherInfo.FirstName} {teacher.TeacherInfo.SecondName}";
                        return (true, null, displayName, "Teacher");
                    }
                }

                // Check Student membership
                if (userRole == "Student")
                {
                    var student = await _context
                        .Students.Include(s => s.StudentInfo)
                        .Include(s => s.Classes)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (student != null && student.Classes.Any(c => c.Id == classId))
                    {
                        var displayName =
                            $"{student.StudentInfo.FirstName} {student.StudentInfo.SecondName}";
                        return (true, null, displayName, "Student");
                    }
                }

                // Check Admin
                if (userRole == "Admin")
                {
                    var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    if (admin != null && admin.Role == UserRole.Admin)
                    {
                        var displayName = $"{admin.FirstName} {admin.SecondName}";
                        return (true, null, displayName, "Admin");
                    }
                }

                // Check Parent membership (for parent joining child's class)
                if (userRole == "Parent")
                {
                    // userId here is actually the studentId (child's ID)
                    var student = await _context
                        .Students.Include(s => s.StudentInfo)
                        .Include(s => s.Classes)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (student != null && student.Classes.Any(c => c.Id == classId))
                    {
                        var displayName = $"Parent of {student.StudentInfo.FirstName}";
                        return (true, null, displayName, "Parent");
                    }
                }

                return (false, "You are not authorized to join this meeting.", null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking meeting access for user {UserId} and class {ClassId}",
                    userId,
                    classId
                );
                return (false, "An error occurred while verifying access.", null, null);
            }
        }

        public async Task<bool> IsTeacherOfClassAsync(Guid teacherId, Guid classId)
        {
            try
            {
                return await _context
                    .Teachers.Where(t => t.UserId == teacherId)
                    .SelectMany(t => t.Classes)
                    .AnyAsync(c => c.Id == classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking teacher {TeacherId} ownership of class {ClassId}",
                    teacherId,
                    classId
                );
                return false;
            }
        }

        public async Task<bool> IsStudentOfClassAsync(Guid studentId, Guid classId)
        {
            try
            {
                return await _context
                    .Students.Where(s => s.UserId == studentId)
                    .SelectMany(s => s.Classes)
                    .AnyAsync(c => c.Id == classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking student {StudentId} enrollment in class {ClassId}",
                    studentId,
                    classId
                );
                return false;
            }
        }

        public async Task<IEnumerable<Class>> GetParentActiveMeetingsAsync(Guid parentId)
        {
            try
            {
                // Get all children of this parent
                var parent = await _context
                    .Parents.Include(p => p.Students)
                    .ThenInclude(s => s.StudentInfo)
                    .Include(p => p.Students)
                    .ThenInclude(s => s.Classes)
                    .ThenInclude(c => c.Teachers)
                    .FirstOrDefaultAsync(p => p.UserId == parentId);

                if (parent == null || !parent.Students.Any())
                {
                    return new List<Class>();
                }

                // Get all unique active classes from all children
                var activeClasses = parent
                    .Students.SelectMany(child => child.Classes)
                    .Where(c => c.IsMeetingActive)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                // Attach student info to each class for display
                foreach (var cls in activeClasses)
                {
                    var studentsInClass = parent
                        .Students.Where(child => child.Classes.Any(c => c.Id == cls.Id))
                        .ToList();
                    cls.Students = studentsInClass;
                }

                return activeClasses;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting active meetings for parent {ParentId}",
                    parentId
                );
                return new List<Class>();
            }
        }

        public async Task<bool> IsParentOfStudentAsync(Guid parentId, Guid studentId)
        {
            try
            {
                return await _context
                    .Parents.Where(p => p.UserId == parentId)
                    .SelectMany(p => p.Students)
                    .AnyAsync(s => s.UserId == studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking parent {ParentId} ownership of student {StudentId}",
                    parentId,
                    studentId
                );
                return false;
            }
        }

        public async Task<Student> GetStudentDetailsAsync(Guid studentId)
        {
            try
            {
                return await _context
                    .Students.Include(s => s.StudentInfo)
                    .Include(s => s.Classes)
                    .FirstOrDefaultAsync(s => s.UserId == studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student {StudentId} details", studentId);
                return null;
            }
        }

        public async Task<Class> GetClassWithDetailsAsync(Guid classId)
        {
            try
            {
                return await _context
                    .Classes.Include(c => c.Teachers)
                    .Include(c => c.Students)
                    .FirstOrDefaultAsync(c => c.Id == classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting class {ClassId} details", classId);
                return null;
            }
        }
    }
}

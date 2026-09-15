using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.DTO.StudentWird;
using Hafiz.Application.Interfaces;
using Hafiz.Application.Mappers;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hafiz.Application.Services
{
    public class StudentWirdService : IStudentWirdService
    {
        private readonly IStudentService _studentService;
        private readonly IStudentRoutinePlanService _planService;
        private readonly IUserTimeProvider _timeProvider;
        private readonly ILogger<StudentWirdService> _logger;

        public StudentWirdService(
            IStudentService studentService,
            IStudentRoutinePlanService planService,
            IUserTimeProvider timeProvider,
            ILogger<StudentWirdService> logger
        )
        {
            _studentService = studentService;
            _planService = planService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<StudentWirdContextDto?> GetStudentWirdContextAsync(Guid studentId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning(
                    "Student with ID {StudentId} was not found when loading wird context",
                    studentId
                );
                return null;
            }

            var plan = await _planService.GetPlanByStudentIdAsync(studentId);

            var allWirds = student.wirds ?? new List<WirdAssignment>();
            var todayDate = _timeProvider.GetUserToday();

            // 1. Calculate the latest recorded wird for each category
            var lastMemorization = GetLatestWird(
                allWirds,
                todayDate,
                w => w.Type == AssignmentType.Memorization
            );

            var lastRecentRev = GetLatestWird(
                allWirds,
                todayDate,
                w => w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz
            );

            var lastOldRev = GetLatestWird(
                allWirds,
                todayDate,
                w => w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz
            );

            var lastRecitation = GetLatestWird(
                allWirds,
                todayDate,
                w => w.Type == AssignmentType.Tajwid
            );

            // 2. Filter today's wirds
            var todayWirds = allWirds.Where(w => w.AssignedDate.Date == todayDate).ToList();

            var todayMem = todayWirds.FirstOrDefault(w => w.Type == AssignmentType.Memorization);
            var todayRecentRev = todayWirds.FirstOrDefault(w =>
                w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz
            );
            var todayOldRev = todayWirds.FirstOrDefault(w =>
                w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz
            );
            var todayRecitation = todayWirds.FirstOrDefault(w => w.Type == AssignmentType.Tajwid);

            // 3. Assemble strongly-typed DTO
            return new StudentWirdContextDto
            {
                Student = StudentWirdMapper.MapStudentSummary(student),
                Plan = plan,
                LastWirds = new LastWirdsDto
                {
                    Memorization = StudentWirdMapper.MapLastWirdItem(lastMemorization),
                    RecentRevision = StudentWirdMapper.MapLastWirdItem(lastRecentRev),
                    OldRevision = StudentWirdMapper.MapLastWirdItem(lastOldRev),
                    Recitation = StudentWirdMapper.MapLastWirdItem(lastRecitation),
                },
                TodayWirds = new TodayWirdsDto
                {
                    Memorization = StudentWirdMapper.MapTodayWirdItem(todayMem),
                    RecentRevision = StudentWirdMapper.MapTodayWirdItem(todayRecentRev),
                    OldRevision = StudentWirdMapper.MapTodayWirdItem(todayOldRev),
                    Recitation = StudentWirdMapper.MapTodayWirdItem(todayRecitation),
                },
            };
        }

        /// <summary>
        /// Finds the most recent assignment for the specified category.
        /// Prefers assignments before today; falls back to the latest assignment overall if none exist prior to today.
        /// </summary>
        private static WirdAssignment? GetLatestWird(
            IEnumerable<WirdAssignment> wirds,
            DateTime todayDate,
            Func<WirdAssignment, bool> categoryPredicate
        )
        {
            return wirds
                    .Where(w => categoryPredicate(w) && w.AssignedDate.Date < todayDate)
                    .OrderByDescending(w => w.AssignedDate)
                    .FirstOrDefault()
                ?? wirds
                    .Where(categoryPredicate)
                    .OrderByDescending(w => w.AssignedDate)
                    .FirstOrDefault();
        }
    }
}

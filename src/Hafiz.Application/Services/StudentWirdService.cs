using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.DTO.StudentWird;
using Hafiz.Application.Interfaces;
using Hafiz.Application.Mappers;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hafiz.Application.Services
{
    public class StudentWirdService : IStudentWirdService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IWirdRepository _wirdRepository;
        private readonly IStudentRoutinePlanService _planService;
        private readonly IUserTimeProvider _timeProvider;
        private readonly ILogger<StudentWirdService> _logger;

        public StudentWirdService(
            IStudentRepository studentRepository,
            IWirdRepository wirdRepository,
            IStudentRoutinePlanService planService,
            IUserTimeProvider timeProvider,
            ILogger<StudentWirdService> logger
        )
        {
            _studentRepository = studentRepository;
            _wirdRepository = wirdRepository;
            _planService = planService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<StudentWirdContextDto?> GetStudentWirdContextAsync(Guid studentId)
        {
            // 1. Lightweight student info lookup without tracking or historical collections
            var student = await _studentRepository.GetStudentBasicByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning(
                    "Student with ID {StudentId} was not found when loading wird context",
                    studentId
                );
                return null;
            }

            var plan = await _planService.GetPlanByStudentIdAsync(studentId);
            var todayDate = _timeProvider.GetUserToday();

            // 2. Fast database-level projection for latest wirds per category using composite index
            var (lastMemorization, lastRecentRev, lastOldRev, lastRecitation) =
                await _wirdRepository.GetLatestWirdsForContextAsync(studentId, todayDate);

            // 3. Fast query for today's active wirds only
            var todayWirds = await _wirdRepository.GetTodayWirdsAsync(studentId, todayDate);

            var todayMem = todayWirds.FirstOrDefault(w => w.Type == AssignmentType.Memorization);
            var todayRecentRev = todayWirds.FirstOrDefault(w =>
                w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz
            );
            var todayOldRev = todayWirds.FirstOrDefault(w =>
                w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz
            );
            var todayRecitation = todayWirds.FirstOrDefault(w => w.Type == AssignmentType.Tajwid);

            // 4. Assemble strongly-typed DTO
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
    }
}

using Hafiz.Data;
using Hafiz.DTOs.Dashboard;
using Hafiz.Infrastructure.Services.Dashboard;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Infrastructure.Services
{
    /// <summary>
    /// مُنسِّق لوحة الإدارة: يجمع الأعداد العامة، ويُفوِّض حساب وحدات الأوراد إلى
    /// <see cref="DashboardStatsCalculator"/>، وجلب آخر الأنشطة إلى <see cref="DashboardActivityQuery"/>.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        internal const int ActivityPageSize = 10;

        private readonly ApplicationDbContext _context;
        private readonly DashboardActivityQuery _activityQuery;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
            _activityQuery = new DashboardActivityQuery(context);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(
            Guid? instituteId = null,
            DashboardPeriod period = DashboardPeriod.AllTime
        )
        {
            var teachersCount = await CountTeachersAsync(instituteId);
            var (maleCount, femaleCount) = await CountStudentsAsync(instituteId);
            var circlesCount = await CountClassesAsync(instituteId);
            var (memPages, memJuz, memAyahs, revPages, revJuz, revAyahs) =
                await AggregateWirdUnitsAsync(instituteId, period);
            var wirdsPage = await _activityQuery.GetTodaysPageAsync(
                instituteId,
                DashboardActivityCategory.Wirds,
                0,
                ActivityPageSize
            );
            var attendancePage = await _activityQuery.GetTodaysPageAsync(
                instituteId,
                DashboardActivityCategory.Attendance,
                0,
                ActivityPageSize
            );
            var (expectedToday, attendedToday) = await ComputeTodayAttendanceAsync(instituteId);
            var (expectedTeachersToday, attendedTeachersToday) =
                await ComputeTodayTeacherAttendanceAsync(instituteId);
            return new DashboardStatsDto
            {
                CirclesCount = circlesCount,
                TeachersCount = teachersCount,
                MaleStudentsCount = maleCount,
                FemaleStudentsCount = femaleCount,
                MemorizationPages = Math.Round(memPages, 2),
                MemorizationJuz = Math.Round(memJuz, 2),
                MemorizationAyahs = memAyahs,
                RevisionPages = Math.Round(revPages, 2),
                RevisionJuzParts = Math.Round(revJuz, 2),
                RevisionAyahs = revAyahs,
                SelectedPeriod = period,
                WirdsActivity = wirdsPage,
                AttendanceActivity = attendancePage,
                ExpectedAttendanceToday = expectedToday,
                AttendedToday = attendedToday,
                ExpectedTeachersToday = expectedTeachersToday,
                AttendedTeachersToday = attendedTeachersToday,
            };
        }

        // دوام اليوم: يعتمد على أيام دوام الحلقة (ClassDays).
        // المتوقع = مجموع أعداد الطلاب في كل حلقة تدرّس اليوم (حصص لا طلاب فريدين).
        // الفعلي = سجلات الحضور اليوم بحالة "حاضر" أو "متأخر" ضمن هذه الحلقات.
        private async Task<(int Expected, int Attended)> ComputeTodayAttendanceAsync(
            Guid? instituteId
        )
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var currentDay = (ClassDaysEnum)((int)today.DayOfWeek + 1);

            var classesToday = _context
                .Classes.Where(c => c.ClassDays.Any(d => d == currentDay))
                .AsNoTracking();
            if (instituteId.HasValue)
                classesToday = classesToday.Where(c => c.InstituteId == instituteId);

            var expected = await classesToday.SumAsync(c => c.Students.Count);

            var attended = await _context
                .StudentAttendances.Where(a =>
                    a.Date >= today
                    && a.Date < tomorrow
                    && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                    && classesToday.Any(c => c.Id == a.ClassId)
                )
                .AsNoTracking()
                .CountAsync();

            return (expected, attended);
        }

        private async Task<(int Expected, int Attended)> ComputeTodayTeacherAttendanceAsync(
            Guid? instituteId
        )
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var currentDay = (ClassDaysEnum)((int)today.DayOfWeek + 1);
            IQueryable<Teacher>? teachersToday = _context
                .Teachers.Where(t => t.Classes.Any(c => c.ClassDays.Any(d => d == currentDay)))
                .AsNoTracking();

            if (instituteId.HasValue)
                teachersToday = teachersToday.Where(t => t.TeacherInfo.InstituteId == instituteId);

            var expected = await teachersToday.CountAsync();
            var attended = await _context
                .teacherAttendances.Where(a =>
                    a.Date >= today
                    && a.Date < tomorrow
                    && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                    && teachersToday.Any(t => t.Attendances.Any(att => att.Id == a.Id))
                )
                .AsNoTracking()
                .CountAsync();
            return (expected, attended);
        }

        public Task<DashboardActivityPage> GetActivityPageAsync(
            Guid? instituteId,
            DashboardActivityCategory category,
            int page,
            int pageSize
        ) => _activityQuery.GetTodaysPageAsync(instituteId, category, page, pageSize);

        private Task<int> CountTeachersAsync(Guid? instituteId) =>
            (
                instituteId.HasValue
                    ? _context.Teachers.Where(t => t.TeacherInfo.InstituteId == instituteId)
                    : _context.Teachers.AsQueryable()
            ).CountAsync();

        private async Task<(int Male, int Female)> CountStudentsAsync(Guid? instituteId)
        {
            var q = instituteId.HasValue
                ? _context.Students.Where(s => s.StudentInfo.InstituteId == instituteId)
                : _context.Students.AsQueryable();
            var male = await q.CountAsync(s => s.sex == Sex.male);
            var female = await q.CountAsync(s => s.sex == Sex.female);
            return (male, female);
        }

        private Task<int> CountClassesAsync(Guid? instituteId) =>
            (
                instituteId.HasValue
                    ? _context.Classes.Where(c => c.InstituteId == instituteId)
                    : _context.Classes.AsQueryable()
            ).CountAsync();

        private async Task<(
            double memPages,
            double memJuz,
            int memAyahs,
            double revPages,
            double revJuz,
            int revAyahs
        )> AggregateWirdUnitsAsync(Guid? instituteId, DashboardPeriod period)
        {
            var wirdsQuery = instituteId.HasValue
                ? _context.WirdAssignments.Where(w =>
                    w.Student.StudentInfo.InstituteId == instituteId
                )
                : _context.WirdAssignments.AsQueryable();

            var (from, toExclusive) = DashboardPeriodRange.Resolve(period);
            if (from.HasValue)
                wirdsQuery = wirdsQuery.Where(w =>
                    w.AssignedDate >= from.Value && w.AssignedDate < toExclusive!.Value
                );

            var assignments = await wirdsQuery
                .Where(w =>
                    w.Type == AssignmentType.Memorization || w.Type == AssignmentType.Revision
                )
                .Select(w => new WirdUnitsProjection
                {
                    Type = w.Type,
                    Amount = w.Amount,
                    AmountUnit = w.AmountUnit,
                    EquivalentPages = w.EquivalentPages,
                    FromPage = w.FromPage,
                    ToPage = w.ToPage,
                    FromJuz = w.FromJuz,
                    ToJuz = w.ToJuz,
                    FromAyah = w.FromAyah,
                    ToAyah = w.ToAyah,
                })
                .ToListAsync();

            double memPages = 0,
                memJuz = 0,
                revPages = 0,
                revJuz = 0;
            int memAyahs = 0,
                revAyahs = 0;

            foreach (var w in assignments)
            {
                if (w.Type == AssignmentType.Memorization)
                    DashboardStatsCalculator.Accumulate(w, ref memPages, ref memJuz, ref memAyahs);
                else
                    DashboardStatsCalculator.Accumulate(w, ref revPages, ref revJuz, ref revAyahs);
            }

            return (memPages, memJuz, memAyahs, revPages, revJuz, revAyahs);
        }
    }
}

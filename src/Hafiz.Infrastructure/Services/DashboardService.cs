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
            DashboardPeriod period = DashboardPeriod.AllTime,
            DateTime? today = null
        )
        {
            var counts = await LoadCountsAsync(instituteId, today);
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
            return new DashboardStatsDto
            {
                CirclesCount = counts.Classes,
                TeachersCount = counts.Teachers,
                MaleStudentsCount = counts.MaleStudents,
                FemaleStudentsCount = counts.FemaleStudents,
                MemorizationPages = Math.Round(memPages, 2),
                MemorizationJuz = Math.Round(memJuz, 2),
                MemorizationAyahs = memAyahs,
                RevisionPages = Math.Round(revPages, 2),
                RevisionJuzParts = Math.Round(revJuz, 2),
                RevisionAyahs = revAyahs,
                SelectedPeriod = period,
                WirdsActivity = wirdsPage,
                AttendanceActivity = attendancePage,
                ExpectedAttendanceToday = counts.ExpectedStudentsToday,
                AttendedToday = counts.AttendedStudentsToday,
                ExpectedTeachersToday = counts.ExpectedTeachersToday,
                AttendedTeachersToday = counts.AttendedTeachersToday,
            };
        }

        /// <summary>
        /// كل الأعداد العددية للوحة تُجلب باستعلام واحد (استعلامات فرعية داخل SELECT)
        /// بدل ثماني رحلات منفصلة إلى قاعدة البيانات.
        ///
        /// دوام اليوم: يعتمد على أيام دوام الحلقة (ClassDays).
        /// المتوقع = مجموع أعداد الطلاب في كل حلقة تدرّس اليوم (حصص لا طلاب فريدين).
        /// الفعلي = سجلات الحضور اليوم بحالة "حاضر" أو "متأخر" ضمن هذه الحلقات.
        /// </summary>
        private Task<DashboardCounts> LoadCountsAsync(Guid? instituteId, DateTime? todayParam)
        {
            var today = todayParam?.Date ?? DateTime.Today;
            var tomorrow = today.AddDays(1);
            var currentDay = (ClassDaysEnum)((int)today.DayOfWeek + 1);

            IQueryable<Teacher> teachers = _context.Teachers;
            IQueryable<Student> students = _context.Students;
            IQueryable<Class> classes = _context.Classes;
            if (instituteId.HasValue)
            {
                teachers = teachers.Where(t => t.TeacherInfo.InstituteId == instituteId);
                students = students.Where(s => s.StudentInfo.InstituteId == instituteId);
                classes = classes.Where(c => c.InstituteId == instituteId);
            }

            var classesToday = classes.Where(c => c.ClassDays.Any(d => d == currentDay));
            var teachersToday = teachers.Where(t =>
                t.Classes.Any(c => c.ClassDays.Any(d => d == currentDay))
            );

            // صفّ واحد يُستخدم كمرساة للاستعلام؛ جدول المستخدمين لا يخلو أبدًا (يُزرع SuperAdmin عند الإقلاع).
            return _context
                .Users.Take(1)
                .Select(_ => new DashboardCounts
                {
                    Teachers = teachers.Count(),
                    MaleStudents = students.Count(s => s.sex == Sex.male),
                    FemaleStudents = students.Count(s => s.sex == Sex.female),
                    Classes = classes.Count(),
                    ExpectedStudentsToday = classesToday.Sum(c => c.Students.Count),
                    AttendedStudentsToday = _context.StudentAttendances.Count(a =>
                        a.Date >= today
                        && a.Date < tomorrow
                        && (
                            a.Status == AttendanceStatus.Present
                            || a.Status == AttendanceStatus.Late
                        )
                        && classesToday.Any(c => c.Id == a.ClassId)
                    ),
                    ExpectedTeachersToday = teachersToday.Count(),
                    AttendedTeachersToday = _context.teacherAttendances.Count(a =>
                        a.Date >= today
                        && a.Date < tomorrow
                        && (
                            a.Status == AttendanceStatus.Present
                            || a.Status == AttendanceStatus.Late
                        )
                        && teachersToday.Any(t => t.UserId == a.TeacherId)
                    ),
                })
                .AsNoTracking()
                .FirstAsync();
        }

        private sealed class DashboardCounts
        {
            public int Teachers { get; set; }
            public int MaleStudents { get; set; }
            public int FemaleStudents { get; set; }
            public int Classes { get; set; }
            public int ExpectedStudentsToday { get; set; }
            public int AttendedStudentsToday { get; set; }
            public int ExpectedTeachersToday { get; set; }
            public int AttendedTeachersToday { get; set; }
        }

        public Task<DashboardActivityPage> GetActivityPageAsync(
            Guid? instituteId,
            DashboardActivityCategory category,
            int page,
            int pageSize
        ) => _activityQuery.GetTodaysPageAsync(instituteId, category, page, pageSize);

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

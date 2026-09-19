using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
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
            var (memPages, memJuz, memAyahs, revPages, revJuz, revAyahs, tajPages, tajJuz, tajAyahs) =
                await AggregateWirdUnitsAsync(instituteId, period);
            var (matnTotal, matnMem, matnRev, matnMud, matnCompleted, matnMemVerses, matnRevVerses, memUnits, revUnits) =
                await AggregateMatnUnitsAsync(instituteId, period);
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
                MatnsCount = counts.Matns,
                StudyProgramsCount = counts.StudyPrograms,
                ActiveStudyProgramsCount = counts.ActiveStudyPrograms,
                MatnsStudiedCount = counts.MatnsStudied,
                MatnsMemorizedCount = counts.MatnsMemorized,
                MatnsExaminedCount = counts.MatnsExamined,
                ExamsPassedCount = counts.ExamsPassed,
                ExamsFailedCount = counts.ExamsFailed,
                MatnTotalAssignments = matnTotal,
                MatnMemorizationAssignments = matnMem,
                MatnRevisionAssignments = matnRev,
                MatnMudarasahAssignments = matnMud,
                MatnCompletedAssignments = matnCompleted,
                MatnMemorizationVerses = matnMemVerses,
                MatnRevisionVerses = matnRevVerses,
                MatnMemorizationUnits = memUnits,
                MatnRevisionUnits = revUnits,
                MemorizationPages = Math.Round(memPages, 2),
                MemorizationJuz = Math.Round(memJuz, 2),
                MemorizationAyahs = memAyahs,
                RevisionPages = Math.Round(revPages, 2),
                RevisionJuzParts = Math.Round(revJuz, 2),
                RevisionAyahs = revAyahs,
                TajwidPages = Math.Round(tajPages, 2),
                TajwidJuz = Math.Round(tajJuz, 2),
                TajwidAyahs = tajAyahs,
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
        private async Task<DashboardCounts> LoadCountsAsync(Guid? instituteId, DateTime? todayParam)
        {
            var today = todayParam?.Date ?? DateTime.Today;
            var tomorrow = today.AddDays(1);
            var currentDay = (ClassDaysEnum)((int)today.DayOfWeek + 1);

            IQueryable<Teacher> teachers = _context.Teachers;
            IQueryable<Student> students = _context.Students;
            IQueryable<Class> classes = _context.Classes;
            IQueryable<Hafiz.Domain.Entities.Matn> matns = _context.Matns.Where(m => !m.IsDeleted);
            IQueryable<Hafiz.Domain.Entities.StudyProgram> programs = _context.StudyPrograms.Where(p => !p.IsDeleted);
            IQueryable<StudentMatnProgress> matnProgresses = _context.StudentMatnProgresses;

            if (instituteId.HasValue)
            {
                teachers = teachers.Where(t => t.TeacherInfo.InstituteId == instituteId);
                students = students.Where(s => s.StudentInfo.InstituteId == instituteId);
                classes = classes.Where(c => c.InstituteId == instituteId);
                matns = matns.Where(m => m.InstituteId == null || m.InstituteId == instituteId);
                programs = programs.Where(p => p.InstituteId == instituteId);
                matnProgresses = matnProgresses.Where(p => p.Student.StudentInfo.InstituteId == instituteId);
            }

            // صفّ واحد يُستخدم كمرساة للاستعلام؛ جدول المستخدمين لا يخلو أبدًا (يُزرع SuperAdmin عند الإقلاع).
            var counts = await _context
                .Users.Take(1)
                .Select(_ => new DashboardCounts
                {
                    Teachers = teachers.Count(),
                    MaleStudents = students.Count(s => s.sex == Sex.male),
                    FemaleStudents = students.Count(s => s.sex == Sex.female),
                    Classes = classes.Count(),
                    Matns = matns.Count(),
                    StudyPrograms = programs.Count(),
                    ActiveStudyPrograms = programs.Count(p => p.IsActive),
                })
                .AsNoTracking()
                .FirstAsync();

            var classesList = await classes
                .Select(c => new { c.Id, c.ClassDays, StudentsCount = c.Students.Count })
                .ToListAsync();

            var todayClasses = classesList
                .Where(c => c.ClassDays != null && c.ClassDays.Contains(currentDay))
                .ToList();

            var todayClassIds = todayClasses.Select(c => c.Id).ToHashSet();
            counts.ExpectedStudentsToday = todayClasses.Sum(c => c.StudentsCount);

            if (todayClassIds.Any())
            {
                counts.AttendedStudentsToday = await _context.StudentAttendances
                    .CountAsync(a =>
                        a.Date >= today
                        && a.Date < tomorrow
                        && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                        && todayClassIds.Contains(a.ClassId)
                        && !a.Student.StudentInfo.IsDeleted
                    );

                var todayTeacherIds = await _context.Classes
                    .Where(c => todayClassIds.Contains(c.Id))
                    .SelectMany(c => c.Teachers.Select(t => t.UserId))
                    .Distinct()
                    .ToListAsync();

                counts.ExpectedTeachersToday = todayTeacherIds.Count;

                if (todayTeacherIds.Any())
                {
                    counts.AttendedTeachersToday = await _context.teacherAttendances
                        .CountAsync(a =>
                            a.Date >= today
                            && a.Date < tomorrow
                            && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                            && !a.Teacher.IsDeleted
                            && todayTeacherIds.Contains(a.TeacherId)
                        );
                }
            }

            var progressList = await matnProgresses
                .Select(p => new { p.StudyStatus, p.MemorizationStatus, p.ExamStatus })
                .ToListAsync();

            counts.MatnsStudied = progressList.Count(p => p.StudyStatus == StudyStatus.Completed);
            counts.MatnsMemorized = progressList.Count(p => p.MemorizationStatus == MemorizationStatus.Memorized);
            counts.MatnsExamined = progressList.Count(p => p.ExamStatus != ExamStatus.NotTested);
            counts.ExamsPassed = progressList.Count(p => p.ExamStatus == ExamStatus.Passed);
            counts.ExamsFailed = progressList.Count(p => p.ExamStatus == ExamStatus.Failed);

            return counts;
        }

        private sealed class DashboardCounts
        {
            public int Teachers { get; set; }
            public int MaleStudents { get; set; }
            public int FemaleStudents { get; set; }
            public int Classes { get; set; }
            public int Matns { get; set; }
            public int StudyPrograms { get; set; }
            public int ActiveStudyPrograms { get; set; }
            public int MatnsStudied { get; set; }
            public int MatnsMemorized { get; set; }
            public int MatnsExamined { get; set; }
            public int ExamsPassed { get; set; }
            public int ExamsFailed { get; set; }
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
            int revAyahs,
            double tajPages,
            double tajJuz,
            int tajAyahs
        )> AggregateWirdUnitsAsync(Guid? instituteId, DashboardPeriod period)
        {
            var wirdsQuery = _context.WirdAssignments.IgnoreQueryFilters().AsNoTracking();
            if (instituteId.HasValue)
                wirdsQuery = wirdsQuery.Where(w =>
                    w.Student.StudentInfo.InstituteId == instituteId
                );

            var (from, toExclusive) = DashboardPeriodRange.Resolve(period);
            if (from.HasValue)
                wirdsQuery = wirdsQuery.Where(w =>
                    w.AssignedDate >= from.Value && w.AssignedDate < toExclusive!.Value
                );

            var assignments = await wirdsQuery
                .Where(w =>
                    w.Status != AssignmentStatus.notSet
                    && (w.Type == AssignmentType.Memorization || w.Type == AssignmentType.Revision || w.Type == AssignmentType.Tajwid)
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
                revJuz = 0,
                tajPages = 0,
                tajJuz = 0;
            int memAyahs = 0,
                revAyahs = 0,
                tajAyahs = 0;

            foreach (var w in assignments)
            {
                if (w.Type == AssignmentType.Memorization)
                    DashboardStatsCalculator.Accumulate(w, ref memPages, ref memJuz, ref memAyahs);
                else if (w.Type == AssignmentType.Revision)
                    DashboardStatsCalculator.Accumulate(w, ref revPages, ref revJuz, ref revAyahs);
                else if (w.Type == AssignmentType.Tajwid)
                    DashboardStatsCalculator.Accumulate(w, ref tajPages, ref tajJuz, ref tajAyahs);
            }

            return (memPages, memJuz, memAyahs, revPages, revJuz, revAyahs, tajPages, tajJuz, tajAyahs);
        }

        private async Task<(
            int totalAssignments,
            int memAssignments,
            int revAssignments,
            int mudAssignments,
            int completedAssignments,
            decimal memVerses,
            decimal revVerses,
            MatnUnitBreakdownDto memUnits,
            MatnUnitBreakdownDto revUnits
        )> AggregateMatnUnitsAsync(Guid? instituteId, DashboardPeriod period)
        {
            var matnQuery = _context.MatnAssignments.AsNoTracking();
            if (instituteId.HasValue)
            {
                matnQuery = matnQuery.Where(m => m.Student.StudentInfo.InstituteId == instituteId);
            }

            var (from, toExclusive) = DashboardPeriodRange.Resolve(period);
            if (from.HasValue)
            {
                matnQuery = matnQuery.Where(m => m.AssignedDate >= from.Value && m.AssignedDate < toExclusive!.Value);
            }

            var list = await matnQuery
                .Select(m => new
                {
                    m.PerformanceType,
                    m.Unit,
                    m.Amount,
                    m.IsCompleted,
                    m.Status,
                })
                .ToListAsync();

            int total = list.Count;
            int mem = list.Count(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization);
            int rev = list.Count(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision);
            int mud = list.Count(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Mudarasah);
            int completed = list.Count(m => m.IsCompleted || m.Status != Hafiz.Models.AssignmentStatus.notSet);

            var memUnits = new MatnUnitBreakdownDto
            {
                Verses = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Unit == Hafiz.Domain.Enums.MatnUnit.Verses && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Lines = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Unit == Hafiz.Domain.Enums.MatnUnit.Lines && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Pages = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Unit == Hafiz.Domain.Enums.MatnUnit.Pages && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Chapters = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Unit == Hafiz.Domain.Enums.MatnUnit.Chapters && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Hadiths = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Unit == Hafiz.Domain.Enums.MatnUnit.Hadiths && m.Amount.HasValue).Sum(m => m.Amount!.Value),
            };

            var revUnits = new MatnUnitBreakdownDto
            {
                Verses = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Unit == Hafiz.Domain.Enums.MatnUnit.Verses && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Lines = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Unit == Hafiz.Domain.Enums.MatnUnit.Lines && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Pages = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Unit == Hafiz.Domain.Enums.MatnUnit.Pages && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Chapters = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Unit == Hafiz.Domain.Enums.MatnUnit.Chapters && m.Amount.HasValue).Sum(m => m.Amount!.Value),
                Hadiths = list.Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Unit == Hafiz.Domain.Enums.MatnUnit.Hadiths && m.Amount.HasValue).Sum(m => m.Amount!.Value),
            };

            decimal memVerses = memUnits.Verses > 0 ? memUnits.Verses : list
                .Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Memorization && m.Amount.HasValue)
                .Sum(m => m.Amount!.Value);

            decimal revVerses = revUnits.Verses > 0 ? revUnits.Verses : list
                .Where(m => m.PerformanceType == Hafiz.Domain.Enums.MatnPerformanceType.Revision && m.Amount.HasValue)
                .Sum(m => m.Amount!.Value);

            return (total, mem, rev, mud, completed, Math.Round(memVerses, 1), Math.Round(revVerses, 1), memUnits, revUnits);
        }
    }
}

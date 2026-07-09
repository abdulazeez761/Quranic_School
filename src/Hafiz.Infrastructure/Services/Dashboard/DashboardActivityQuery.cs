using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.DTOs.Dashboard;
using Hafiz.Models;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// يجلب أنشطة اليوم في مراكز التحفيظ (أوراد جديدة، تسجيل حضور طلاب/معلمين)
    /// لعرضها ضمن قسمَين منفصلَين على لوحة الإدارة، مع دعم التحميل التدريجي (Load more).
    /// حضور الطلاب مُجمَّع على مستوى الحلقة+التاريخ حتى لا تُغرق القائمة بسجل لكل طالب.
    /// </summary>
    internal sealed class DashboardActivityQuery
    {
        // حد أمان لكل مصدر لمنع سحب أعداد ضخمة عند الأيام النادرة النشاط العالي جدًا.
        private const int PerSourceCap = 500;

        private readonly ApplicationDbContext _context;

        public DashboardActivityQuery(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardActivityPage> GetTodaysPageAsync(
            Guid? instituteId,
            DashboardActivityCategory category,
            int page,
            int pageSize
        )
        {
            page = Math.Max(0, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var (from, toExclusive) = TodayRange();

            var all =
                category == DashboardActivityCategory.Wirds
                    ? await FetchWirdsAsync(instituteId, from, toExclusive)
                    : await FetchAttendanceAsync(instituteId, from, toExclusive);

            all = all.OrderByDescending(a => a.Timestamp).ToList();
            var slice = all.Skip(page * pageSize).Take(pageSize).ToList();

            return new DashboardActivityPage
            {
                Category = category,
                Items = slice,
                HasMore = all.Count > (page + 1) * pageSize,
                NextPage = page + 1,
                TotalToday = all.Count,
            };
        }

        private async Task<List<DashboardActivityItem>> FetchWirdsAsync(
            Guid? instituteId,
            DateTime from,
            DateTime toExclusive
        )
        {
            var query = instituteId.HasValue
                ? _context.WirdAssignments.Where(w =>
                    w.Student.StudentInfo.InstituteId == instituteId
                )
                : _context.WirdAssignments.AsQueryable();

            var rows = await query
                .Where(w => w.AssignedDate >= from && w.AssignedDate < toExclusive)
                .OrderByDescending(w => w.AssignedDate)
                .Take(PerSourceCap)
                .Select(w => new
                {
                    w.Type,
                    w.Amount,
                    w.AmountUnit,
                    w.AssignedDate,
                    First = w.Student.StudentInfo.FirstName,
                    Second = w.Student.StudentInfo.SecondName,
                    // The class the wird was assigned within. Legacy rows without a
                    // ClassId fall back to any of the student's classes so the feed row
                    // still has a class label.
                    ClassName = w.Class != null
                        ? w.Class.Name
                        : w
                            .Student.Classes.OrderBy(c => c.Name)
                            .Select(c => c.Name)
                            .FirstOrDefault(),
                })
                .ToListAsync();

            return rows.Select(r => new DashboardActivityItem
                {
                    Kind =
                        r.Type == AssignmentType.Memorization
                            ? DashboardActivityKind.WirdMemorization
                            : DashboardActivityKind.WirdRevision,
                    Title =
                        r.Type == AssignmentType.Memorization ? "ورد حفظ جديد" : "ورد مراجعة جديد",
                    Subtitle = DashboardActivityFormatter.BuildWirdSubtitle(
                        r.First,
                        r.Second,
                        r.ClassName,
                        r.Amount,
                        r.AmountUnit
                    ),
                    Timestamp = r.AssignedDate,
                })
                .ToList();
        }

        private async Task<List<DashboardActivityItem>> FetchAttendanceAsync(
            Guid? instituteId,
            DateTime from,
            DateTime toExclusive
        )
        {
            var students = await FetchStudentAttendanceAsync(instituteId, from, toExclusive);
            var teachers = await FetchTeacherAttendanceAsync(instituteId, from, toExclusive);
            return students.Concat(teachers).ToList();
        }

        private async Task<IEnumerable<DashboardActivityItem>> FetchStudentAttendanceAsync(
            Guid? instituteId,
            DateTime from,
            DateTime toExclusive
        )
        {
            IQueryable<StudentAttendance>? query = instituteId.HasValue
                ? _context.StudentAttendances.Where(a =>
                    a.Student.StudentInfo.InstituteId == instituteId
                )
                : _context.StudentAttendances.AsQueryable();

            var rows = await query
                .Where(a => a.Date >= from && a.Date < toExclusive)
                .GroupBy(a => new
                {
                    a.ClassId,
                    a.Class.Name,
                    a.Date,
                })
                .OrderByDescending(g => g.Key.Date)
                .Take(PerSourceCap)
                .Select(g => new
                {
                    total = g.FirstOrDefault()!.Class.Students.Count(),
                    g.Key.Name,
                    g.Key.Date,
                    Present = g.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = g.Count(a => a.Status == AttendanceStatus.Absent),
                    Excused = g.Count(a => a.Status == AttendanceStatus.Excused),
                    Late = g.Count(a => a.Status == AttendanceStatus.Late),
                })
                .ToListAsync();

            return rows.Select(r => new DashboardActivityItem
            {
                Kind = DashboardActivityKind.StudentAttendance,
                Title = "تسجيل حضور طلاب",
                Subtitle = $"حلقة {r.Name}",
                Counts = new AttendanceCountsSummary
                {
                    Present = r.Present,
                    Absent = r.Absent,
                    Excused = r.Excused,
                    Late = r.Late,
                    Total = r.total,
                },
                Timestamp = r.Date,
            });
        }

        private async Task<IEnumerable<DashboardActivityItem>> FetchTeacherAttendanceAsync(
            Guid? instituteId,
            DateTime from,
            DateTime toExclusive
        )
        {
            var query = instituteId.HasValue
                ? _context.teacherAttendances.Where(a =>
                    a.Teacher.TeacherInfo.InstituteId == instituteId
                )
                : _context.teacherAttendances.AsQueryable();

            var rows = await query
                .Where(a => a.Date >= from && a.Date < toExclusive)
                .OrderByDescending(a => a.Date)
                .Take(PerSourceCap)
                .Select(a => new
                {
                    First = a.Teacher.TeacherInfo.FirstName,
                    Second = a.Teacher.TeacherInfo.SecondName,
                    ClassName = a.Class.Name,
                    a.Date,
                    a.Status,
                })
                .ToListAsync();

            return rows.Select(r => new DashboardActivityItem
            {
                Kind = DashboardActivityKind.TeacherAttendance,
                Title = "حضور معلم",
                Subtitle = DashboardActivityFormatter.BuildTeacherAttendanceSubtitle(
                    r.First,
                    r.Second,
                    r.ClassName
                ),
                TeacherStatus = r.Status,
                Timestamp = r.Date,
            });
        }

        private static (DateTime from, DateTime toExclusive) TodayRange()
        {
            var today = DateTime.Today;
            return (today, today.AddDays(1));
        }
    }
}

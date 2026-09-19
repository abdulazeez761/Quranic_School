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
                    ClassName = w.Class != null
                        ? w.Class.Name
                        : w
                            .Student.Classes.OrderBy(c => c.Name)
                            .Select(c => c.Name)
                            .FirstOrDefault(),
                })
                .ToListAsync();

            var wirdItems = rows.Select(r => new DashboardActivityItem
            {
                Kind = r.Type switch
                {
                    AssignmentType.Memorization => DashboardActivityKind.WirdMemorization,
                    AssignmentType.Revision => DashboardActivityKind.WirdRevision,
                    AssignmentType.Tajwid => DashboardActivityKind.WirdTajwid,
                    _ => DashboardActivityKind.WirdRevision,
                },
                Title = r.Type switch
                {
                    AssignmentType.Memorization => "ورد حفظ جديد",
                    AssignmentType.Revision => "ورد مراجعة جديد",
                    AssignmentType.Tajwid => "ورد تجويد جديد",
                    _ => "ورد جديد",
                },
                Subtitle = DashboardActivityFormatter.BuildWirdSubtitle(
                    r.First,
                    r.Second,
                    r.ClassName,
                    r.Amount,
                    r.AmountUnit
                ),
                Timestamp = r.AssignedDate,
            });

            // جلب أوراد المتون العلمية (حفظ، مراجعة، مدارسة) لليوم
            var matnQuery = instituteId.HasValue
                ? _context.MatnAssignments.Where(m =>
                    m.Student.StudentInfo.InstituteId == instituteId
                )
                : _context.MatnAssignments.AsQueryable();

            var matnRows = await matnQuery
                .Where(m => m.AssignedDate >= from && m.AssignedDate < toExclusive)
                .OrderByDescending(m => m.AssignedDate)
                .Take(PerSourceCap)
                .Select(m => new
                {
                    m.PerformanceType,
                    m.Amount,
                    m.Unit,
                    m.ChapterName,
                    m.FromNumber,
                    m.ToNumber,
                    m.AssignedDate,
                    First = m.Student.StudentInfo.FirstName,
                    Second = m.Student.StudentInfo.SecondName,
                    ClassName = m.Class != null ? m.Class.Name : "",
                })
                .ToListAsync();

            var matnItems = matnRows.Select(m =>
            {
                var kind = m.PerformanceType switch
                {
                    Hafiz.Domain.Enums.MatnPerformanceType.Memorization => DashboardActivityKind.MatnMemorization,
                    Hafiz.Domain.Enums.MatnPerformanceType.Revision => DashboardActivityKind.MatnRevision,
                    Hafiz.Domain.Enums.MatnPerformanceType.Mudarasah => DashboardActivityKind.MatnMudarasah,
                    _ => DashboardActivityKind.MatnMemorization,
                };
                var title = m.PerformanceType switch
                {
                    Hafiz.Domain.Enums.MatnPerformanceType.Memorization => "ورد حفظ متن",
                    Hafiz.Domain.Enums.MatnPerformanceType.Revision => "ورد مراجعة متن",
                    Hafiz.Domain.Enums.MatnPerformanceType.Mudarasah => "ورد مدارسة متن",
                    _ => "ورد متن",
                };

                string unitName = m.Unit switch
                {
                    Hafiz.Domain.Enums.MatnUnit.Verses => "أبيات",
                    Hafiz.Domain.Enums.MatnUnit.Lines => "سطور",
                    Hafiz.Domain.Enums.MatnUnit.Pages => "صفحات",
                    Hafiz.Domain.Enums.MatnUnit.Chapters => "أبواب",
                    Hafiz.Domain.Enums.MatnUnit.Hadiths => "أحاديث",
                    _ => "وحدات",
                };

                var details = m.Amount.HasValue ? $"{m.Amount.Value:0.##} {unitName}" : "";
                if (m.FromNumber.HasValue && m.ToNumber.HasValue)
                {
                    details = $"{details} (من {m.FromNumber} إلى {m.ToNumber})".Trim();
                }
                if (!string.IsNullOrWhiteSpace(m.ChapterName))
                {
                    details = string.IsNullOrEmpty(details) ? m.ChapterName : $"{m.ChapterName} · {details}";
                }

                var studentName = $"{m.First} {m.Second}".Trim();
                var classPart = string.IsNullOrWhiteSpace(m.ClassName) ? "" : $" · حلقة {m.ClassName}";
                var subtitle = $"{studentName}{classPart} · {details}".Trim(' ', '·');

                return new DashboardActivityItem
                {
                    Kind = kind,
                    Title = title,
                    Subtitle = subtitle,
                    Timestamp = m.AssignedDate,
                };
            });

            return wirdItems.Concat(matnItems).OrderByDescending(x => x.Timestamp).ToList();
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

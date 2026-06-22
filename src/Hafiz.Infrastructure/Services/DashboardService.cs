using Hafiz.Data;
using Hafiz.DTOs.Dashboard;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(
            Guid? instituteId = null,
            DashboardPeriod period = DashboardPeriod.AllTime
        )
        {
            // ── قاعدة الاستعلام: مُصفَّاة بالمركز إن وُجد ──────────────────
            var teachersQuery = instituteId.HasValue
                ? _context.Teachers.Where(t => t.TeacherInfo.InstituteId == instituteId)
                : _context.Teachers.AsQueryable();

            var studentsQuery = instituteId.HasValue
                ? _context.Students.Where(s => s.StudentInfo.InstituteId == instituteId)
                : _context.Students.AsQueryable();

            var wirdsQuery = instituteId.HasValue
                ? _context.WirdAssignments.Where(w =>
                    w.Student.StudentInfo.InstituteId == instituteId
                )
                : _context.WirdAssignments.AsQueryable();

            var classesQuery = instituteId.HasValue
                ? _context.Classes.Where(c => c.InstituteId == instituteId)
                : _context.Classes.AsQueryable();

            // ── فلترة الأوراد بالفترة الزمنية (على تاريخ الإسناد) ──────────────
            var (from, toExclusive) = GetDateRange(period);
            if (from.HasValue)
                wirdsQuery = wirdsQuery.Where(w =>
                    w.AssignedDate >= from.Value && w.AssignedDate < toExclusive!.Value
                );

            // ── الأعداد العامة (غير مرتبطة بالفترة) ───────────────────────────
            var teachersCount = await teachersQuery.CountAsync();
            var maleCount = await studentsQuery.CountAsync(s => s.sex == Sex.male);
            var femaleCount = await studentsQuery.CountAsync(s => s.sex == Sex.female);
            var circlesCount = await classesQuery.CountAsync();

            // ── جلب الأوراد المطلوبة للإحصاء (حفظ + مراجعة) ───────────────────
            var assignments = await wirdsQuery
                .Where(w =>
                    w.Type == AssignmentType.Memorization || w.Type == AssignmentType.Revision
                )
                .Select(w => new WirdUnitsProjection
                {
                    Type = w.Type,
                    Amount = w.Amount,
                    AmountUnit = w.AmountUnit,
                    FromPage = w.FromPage,
                    ToPage = w.ToPage,
                    FromJuz = w.FromJuz,
                    ToJuz = w.ToJuz,
                    FromAyah = w.FromAyah,
                    ToAyah = w.ToAyah,
                })
                .ToListAsync();

            // ── تجميع الوحدات حسب النوع ───────────────────────────────────────
            double memPages = 0,
                memJuz = 0;
            double revPages = 0,
                revJuz = 0;
            int memAyahs = 0,
                revAyahs = 0;

            foreach (var w in assignments)
            {
                if (w.Type == AssignmentType.Memorization)
                    AccumulateUnits(w, ref memPages, ref memJuz, ref memAyahs);
                else
                    AccumulateUnits(w, ref revPages, ref revJuz, ref revAyahs);
            }

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
            };
        }

        /// <summary>
        /// يحسب مساهمة وردٍ واحد في الصفحات/الأجزاء/الآيات.
        /// الأولوية للقيمة الصريحة (Amount + AmountUnit) — وهي المُدخل الأساسي عند إسناد الورد —
        /// ثم يرجع للنطاق (من/إلى) كحل احتياطي للسجلات القديمة التي لا تحمل Amount.
        /// </summary>
        private static void AccumulateUnits(
            WirdUnitsProjection w,
            ref double pages,
            ref double juz,
            ref int ayahs
        )
        {
            // المُدخل الأساسي: الكمية + الوحدة
            if (w.Amount.HasValue && w.AmountUnit.HasValue)
            {
                switch (w.AmountUnit.Value)
                {
                    case WirdUnit.Pages:
                        pages += (double)w.Amount.Value;
                        return;
                    case WirdUnit.Juz:
                        juz += (double)w.Amount.Value;
                        return;
                    case WirdUnit.Ayahs:
                        ayahs += (int)Math.Round(w.Amount.Value, MidpointRounding.AwayFromZero);
                        return;
                }
            }

            // احتياطي: اشتقاق الكمية من النطاق (شامل للطرفين: من ص1 إلى ص5 = 5 صفحات)
            if (w.FromPage.HasValue && w.ToPage.HasValue)
            {
                pages += Math.Max(0, w.ToPage.Value - w.FromPage.Value + 1);
                return;
            }

            if (w.FromJuz.HasValue && w.ToJuz.HasValue)
            {
                juz += Math.Max(0, w.ToJuz.Value - w.FromJuz.Value + 1);
                return;
            }

            if (
                !string.IsNullOrWhiteSpace(w.FromAyah)
                && w.ToAyah.HasValue
                && int.TryParse(w.FromAyah, out int fromAyahNum)
            )
            {
                ayahs += Math.Max(0, w.ToAyah.Value - fromAyahNum + 1);
            }
        }

        /// <summary>
        /// يُرجع نطاق التاريخ [from, toExclusive) المقابل للفترة المطلوبة.
        /// from == null تعني "بدون فلترة" (إجمالي تراكمي).
        /// </summary>
        private static (DateTime? from, DateTime? toExclusive) GetDateRange(DashboardPeriod period)
        {
            var today = DateTime.Today;
            return period switch
            {
                DashboardPeriod.Today => (today, today.AddDays(1)),
                DashboardPeriod.Month => (
                    new DateTime(today.Year, today.Month, 1),
                    new DateTime(today.Year, today.Month, 1).AddMonths(1)
                ),
                _ => (null, null),
            };
        }

        /// <summary>إسقاط مبسَّط للحقول اللازمة لحساب وحدات الورد.</summary>
        private sealed class WirdUnitsProjection
        {
            public AssignmentType Type { get; init; }
            public decimal? Amount { get; init; }
            public WirdUnit? AmountUnit { get; init; }
            public int? FromPage { get; init; }
            public int? ToPage { get; init; }
            public int? FromJuz { get; init; }
            public int? ToJuz { get; init; }
            public string? FromAyah { get; init; }
            public int? ToAyah { get; init; }
        }
    }
}

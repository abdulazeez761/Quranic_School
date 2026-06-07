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

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? instituteId = null)
        {
            // ── قاعدة الاستعلام: مُصفَّاة بالمركز إن وُجد ──────────────────
            var teachersQuery = instituteId.HasValue
                ? _context.Teachers.Where(t => t.TeacherInfo.InstituteId == instituteId)
                : _context.Teachers.AsQueryable();

            var studentsQuery = instituteId.HasValue
                ? _context.Students.Where(s => s.StudentInfo.InstituteId == instituteId)
                : _context.Students.AsQueryable();

            var wirdsQuery = instituteId.HasValue
                ? _context.WirdAssignments.Where(w => w.Student.StudentInfo.InstituteId == instituteId)
                : _context.WirdAssignments.AsQueryable();

            var classesQuery = instituteId.HasValue
                ? _context.Classes.Where(c => c.InstituteId == instituteId)
                : _context.Classes.AsQueryable();

            // ── عدد المعلمين ────────────────────────────────────────────────
            var teachersCount = await teachersQuery.CountAsync();

            // ── عدد الطلاب الذكور والإناث ──────────────────────────────────
            var maleCount   = await studentsQuery.CountAsync(s => s.sex == Sex.male);
            var femaleCount = await studentsQuery.CountAsync(s => s.sex == Sex.female);

            // ── صفحات الحفظ ─────────────────────────────────────────────────
            var memorizationAssignments = await wirdsQuery
                .Where(w => w.Type == AssignmentType.Memorization)
                .Select(w => new { w.FromPage, w.ToPage, w.FromAyah, w.ToAyah })
                .ToListAsync();

            // الحساب: من ص1 إلى ص21 = 21 - 1 = 20 صفحة (بدون +1)
            int memorizationPages = memorizationAssignments
                .Where(w => w.FromPage.HasValue && w.ToPage.HasValue)
                .Sum(w => w.ToPage!.Value - w.FromPage!.Value);

            // إذا لم توجد صفحات كاملة → نحسب الآيات المتفرقة
            int memorizationAyahs = 0;
            if (memorizationPages == 0)
            {
                memorizationAyahs = memorizationAssignments
                    .Where(w => (!w.FromPage.HasValue || !w.ToPage.HasValue)
                                && w.FromAyah != null && w.ToAyah.HasValue)
                    .Sum(w =>
                    {
                        if (int.TryParse(w.FromAyah, out int fromAyahNum))
                            return Math.Max(0, w.ToAyah!.Value - fromAyahNum + 1);
                        return 0;
                    });
            }

            // ── صفحات ومراجعة الأجزاء ──────────────────────────────────────
            var revisionAssignments = await wirdsQuery
                .Where(w => w.Type == AssignmentType.Revision)
                .Select(w => new { w.FromPage, w.ToPage, w.FromJuz, w.ToJuz })
                .ToListAsync();

            // المراجعة بالصفحات: تحويل صفحات إلى أجزاء
            int revisionPages = revisionAssignments
                .Where(w => w.FromPage.HasValue && w.ToPage.HasValue)
                .Sum(w => w.ToPage!.Value - w.FromPage!.Value);

            // المراجعة بالأجزاء المستقلة: من جزء X إلى جزء Y
            // مثال: من جزء 2 إلى جزء 4 = 3 أجزاء (ToJuz - FromJuz + 1)
            double revisionJuzParts = revisionAssignments
                .Where(w => w.FromJuz.HasValue && w.ToJuz.HasValue)
                .Sum(w => (double)(w.ToJuz!.Value - w.FromJuz!.Value + 1));

            // ── عدد الحلقات ─────────────────────────────────────────────────
            var circlesCount = await classesQuery.CountAsync();

            return new DashboardStatsDto
            {
                CirclesCount        = circlesCount,
                TeachersCount       = teachersCount,
                MaleStudentsCount   = maleCount,
                FemaleStudentsCount = femaleCount,
                MemorizationPages   = memorizationPages,
                MemorizationAyahs   = memorizationAyahs,
                RevisionPages       = revisionPages,
                RevisionJuzParts    = revisionJuzParts,
            };
        }
    }
}

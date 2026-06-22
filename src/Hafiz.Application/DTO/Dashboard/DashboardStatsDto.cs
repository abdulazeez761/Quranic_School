namespace Hafiz.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        /// <summary>عدد الصفحات في الجزء الواحد (مصحف المدينة).</summary>
        private const double PagesPerJuz = 20.0;

        /// <summary>عدد الحلقات</summary>
        public int CirclesCount { get; set; }

        /// <summary>عدد المعلمين</summary>
        public int TeachersCount { get; set; }

        /// <summary>عدد الطلاب الذكور</summary>
        public int MaleStudentsCount { get; set; }

        /// <summary>عدد الطلاب الإناث</summary>
        public int FemaleStudentsCount { get; set; }

        // ── الحفظ ─────────────────────────────────────────────────────────────

        /// <summary>إجمالي صفحات الحفظ (الأوراد المُسجَّلة بوحدة "صفحات").</summary>
        public double MemorizationPages { get; set; }

        /// <summary>عدد أجزاء الحفظ المُسجَّلة مباشرةً بوحدة "أجزاء".</summary>
        public double MemorizationJuz { get; set; }

        /// <summary>عدد آيات الحفظ المُسجَّلة بوحدة "آيات".</summary>
        public int MemorizationAyahs { get; set; }

        // ── المراجعة ──────────────────────────────────────────────────────────

        /// <summary>إجمالي صفحات المراجعة (الأوراد المُسجَّلة بوحدة "صفحات").</summary>
        public double RevisionPages { get; set; }

        /// <summary>
        /// عدد أجزاء المراجعة المُسجَّلة بوحدة "أجزاء" (مثال: من جزء 1 إلى جزء 5 = 5 أجزاء).
        /// </summary>
        public double RevisionJuzParts { get; set; }

        /// <summary>عدد آيات المراجعة المُسجَّلة بوحدة "آيات".</summary>
        public int RevisionAyahs { get; set; }

        // ── الإجماليات المحسوبة (توحيد إلى أجزاء) ───────────────────────────────

        /// <summary>
        /// إجمالي أجزاء الحفظ = (صفحات الحفظ ÷ 20) + الأجزاء المُسجَّلة بوحدة أجزاء.
        /// </summary>
        public double MemorizationParts =>
            Math.Round(MemorizationPages / PagesPerJuz + MemorizationJuz, 2);

        /// <summary>
        /// إجمالي أجزاء المراجعة = (صفحات المراجعة ÷ 20) + الأجزاء المُسجَّلة بوحدة أجزاء.
        /// </summary>
        public double RevisionParts =>
            Math.Round(RevisionPages / PagesPerJuz + RevisionJuzParts, 2);

        /// <summary>الفترة الزمنية المُطبَّقة على إحصائيات الأوراد (لإبراز الزر المختار في الواجهة).</summary>
        public DashboardPeriod SelectedPeriod { get; set; } = DashboardPeriod.AllTime;
    }
}

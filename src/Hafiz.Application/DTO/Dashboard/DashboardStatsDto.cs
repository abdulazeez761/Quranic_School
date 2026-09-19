using System.Collections.Generic;

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

        // ── المتون والبرامج العلمية ──────────────────────────────────────────

        /// <summary>عدد المتون المتاحة في المكتبة للمركز.</summary>
        public int MatnsCount { get; set; }

        /// <summary>إجمالي البرامج التعليمية والعلمية في المركز.</summary>
        public int StudyProgramsCount { get; set; }

        /// <summary>البرامج التعليمية النشطة في المركز.</summary>
        public int ActiveStudyProgramsCount { get; set; }

        // ── أوراد المتون العلمية (للفترة المختارة) ──────────────────────────

        /// <summary>إجمالي أوراد المتون المُسندة للفترة.</summary>
        public int MatnTotalAssignments { get; set; }

        /// <summary>أوراد حفظ المتون للفترة.</summary>
        public int MatnMemorizationAssignments { get; set; }

        /// <summary>أوراد مراجعة المتون للفترة.</summary>
        public int MatnRevisionAssignments { get; set; }

        /// <summary>أوراد مدارسة وشرح المتون للفترة.</summary>
        public int MatnMudarasahAssignments { get; set; }

        /// <summary>أوراد المتون المكتملة للفترة.</summary>
        public int MatnCompletedAssignments { get; set; }

        /// <summary>إجمالي أبيات حفظ المتون المنجزة للفترة.</summary>
        public decimal MatnMemorizationVerses { get; set; }

        /// <summary>إجمالي أبيات مراجعة المتون المنجزة للفترة.</summary>
        public decimal MatnRevisionVerses { get; set; }

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

        // ── التجويد ────────────────────────────────────────────────────────────

        /// <summary>إجمالي صفحات التجويد (الأوراد المُسجَّلة بوحدة "صفحات" أو ما يعادلها).</summary>
        public double TajwidPages { get; set; }

        /// <summary>عدد أجزاء التجويد المُسجَّلة بوحدة "أجزاء".</summary>
        public double TajwidJuz { get; set; }

        /// <summary>عدد آيات التجويد المُسجَّلة بوحدة "آيات".</summary>
        public int TajwidAyahs { get; set; }

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

        /// <summary>
        /// إجمالي أجزاء التجويد = (صفحات التجويد ÷ 20) + الأجزاء المُسجَّلة بوحدة أجزاء.
        /// </summary>
        public double TajwidParts =>
            Math.Round(TajwidPages / PagesPerJuz + TajwidJuz, 2);

        /// <summary>الفترة الزمنية المُطبَّقة على إحصائيات الأوراد (لإبراز الزر المختار في الواجهة).</summary>
        public DashboardPeriod SelectedPeriod { get; set; } = DashboardPeriod.AllTime;

        /// <summary>الصفحة الأولى من أوراد اليوم (حفظ + مراجعة).</summary>
        public DashboardActivityPage WirdsActivity { get; set; } = new();

        /// <summary>الصفحة الأولى من حضور اليوم (طلاب + معلمين).</summary>
        public DashboardActivityPage AttendanceActivity { get; set; } = new();

        // ── دوام اليوم (per-slot: طالب × حلقة تدرّس اليوم) ─────────────────────

        /// <summary>
        /// إجمالي الحضور المتوقع اليوم = مجموع أعداد الطلاب في الحلقات التي تدرّس اليوم
        /// حسب ايام الدوام. يُحسب لكل حصة على حدة (الطالب المسجّل في حلقتين تدرّسان اليوم يُحسب مرتين).
        /// </summary>
        public int ExpectedAttendanceToday { get; set; }

        /// <summary>
        /// عدد الحضور الفعلي المسجّل اليوم (حاضر + متأخر) في الحلقات التي تدرّس اليوم.
        /// </summary>
        public int AttendedToday { get; set; }

        /// <summary>
        /// إجمالي الحضور المتوقع للمعلمين اليوم = مجموع أعداد المعلمين في الحلقات التي تدرّس اليوم
        /// حسب ايام الدوام. يُحسب لكل حصة على حدة (المعلم المسجّل في حلقتين تدرّسان اليوم يُحسب مرتين).
        /// </summary>
        public int ExpectedTeachersToday { get; set; }

        /// <summary>
        /// عدد الحضور الفعلي للمعلمين المسجّل اليوم (حاضر + متأخر) في الحلقات التي تدرّس اليوم.
        /// </summary>
        public int AttendedTeachersToday { get; set; }
    }
}

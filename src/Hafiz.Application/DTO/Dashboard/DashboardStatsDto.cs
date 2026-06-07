namespace Hafiz.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        /// <summary>عدد الحلقات</summary>
        public int CirclesCount { get; set; }

        /// <summary>عدد المعلمين</summary>
        public int TeachersCount { get; set; }

        /// <summary>عدد الطلاب الذكور</summary>
        public int MaleStudentsCount { get; set; }

        /// <summary>عدد الطلاب الإناث</summary>
        public int FemaleStudentsCount { get; set; }

        /// <summary>
        /// إجمالي صفحات الحفظ المكتملة اليوم.
        /// إذا كانت صفراً، يُعرض عدد الآيات بدلاً منها.
        /// </summary>
        public int MemorizationPages { get; set; }

        /// <summary>
        /// عدد آيات الحفظ المتفرقة (تُستخدم عندما تكون MemorizationPages == 0).
        /// </summary>
        public int MemorizationAyahs { get; set; }

        /// <summary>عدد صفحات المراجعة المكتملة اليوم</summary>
        public int RevisionPages { get; set; }

        /// <summary>
        /// عدد الأجزاء المستقلة من المراجعة (من جزء إلى جزء)
        /// مثال: من جزء 2 إلى جزء 4 = 3 أجزاء
        /// </summary>
        public double RevisionJuzParts { get; set; }

        /// <summary>
        /// عدد أجزاء الحفظ (كل 20 صفحة = جزء واد)
        /// الحساب: ToPage - FromPage (من ص1 إلى ص21 = 20 صفحة)
        /// </summary>
        public double MemorizationParts => MemorizationPages > 0 ? Math.Round((double)MemorizationPages / 20.0, 2) : 0;

        /// <summary>
        /// إجمالي أجزاء المراجعة = أجزاء الصفحات + الأجزاء المستقلة
        /// </summary>
        public double RevisionParts => Math.Round((RevisionPages > 0 ? (double)RevisionPages / 20.0 : 0) + RevisionJuzParts, 2);
    }
}

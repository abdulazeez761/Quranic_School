using System.Collections.Generic;

namespace Hafiz.DTOs.Dashboard
{
    /// <summary>
    /// صفحة من قائمة النشاطات لفئة معينة (أوراد أو حضور) خلال اليوم.
    /// تُرجع للواجهة عناصر الصفحة الحالية، مع علم إذا كان هناك المزيد لعرض زر "تحميل المزيد".
    /// </summary>
    public sealed class DashboardActivityPage
    {
        /// <summary>فئة النشاط المُمثَّلة في هذه الصفحة.</summary>
        public DashboardActivityCategory Category { get; init; }

        /// <summary>عناصر الصفحة الحالية بعد الفرز التنازلي حسب الوقت.</summary>
        public IReadOnlyList<DashboardActivityItem> Items { get; init; } =
            new List<DashboardActivityItem>();

        /// <summary>هل تبقّى المزيد من العناصر لهذا اليوم بعد هذه الصفحة؟</summary>
        public bool HasMore { get; init; }

        /// <summary>رقم الصفحة التالية (يستخدمه زر التحميل الإضافي).</summary>
        public int NextPage { get; init; }

        /// <summary>إجمالي عدد الأحداث لهذا اليوم لهذه الفئة (لعرض عدّاد "N حدث اليوم").</summary>
        public int TotalToday { get; init; }
    }
}

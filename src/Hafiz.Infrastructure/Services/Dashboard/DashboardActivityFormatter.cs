using Hafiz.Models;

namespace Hafiz.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// يُحوِّل الحقول الخام (اسم الطالب، الحلقة، الكمية) إلى نصوص عربية مقروءة تُعرض
    /// كسطر ثانوي تحت عناوين عناصر النشاط. مُفصولة عن الاستعلام لتسهيل التعديل.
    /// </summary>
    internal static class DashboardActivityFormatter
    {
        public static string BuildWirdSubtitle(
            string first,
            string second,
            string? className,
            decimal? amount,
            WirdUnit? unit
        )
        {
            var studentName = $"{first} {second}".Trim();
            var head = string.IsNullOrWhiteSpace(className)
                ? studentName
                : $"{studentName} — حلقة {className}";
            if (amount is null || unit is null)
                return head;

            var unitLabel = unit switch
            {
                WirdUnit.Pages => "صفحة",
                WirdUnit.Juz => "جزء",
                WirdUnit.Ayahs => "آية",
                _ => string.Empty,
            };
            var pretty =
                amount.Value % 1 == 0 ? amount.Value.ToString("0") : amount.Value.ToString("0.##");
            return $"{head} — {pretty} {unitLabel}";
        }

        // حالة المعلم تُعرض في الواجهة كشارة ملوّنة، لذا لم تعد تُدرج ضمن النص هنا.
        public static string BuildTeacherAttendanceSubtitle(
            string first,
            string second,
            string className
        )
        {
            var teacherName = $"{first} {second}".Trim();
            return $"{teacherName} — حلقة {className}";
        }
    }
}

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

        public static string BuildTeacherAttendanceSubtitle(
            string first,
            string second,
            string className,
            AttendanceStatus status
        )
        {
            var teacherName = $"{first} {second}".Trim();
            return $"{teacherName} — حلقة {className} ({FormatStatus(status)})";
        }

        private static string FormatStatus(AttendanceStatus status) =>
            status switch
            {
                AttendanceStatus.Present => "حاضر",
                AttendanceStatus.Absent => "غائب",
                AttendanceStatus.Late => "متأخر",
                AttendanceStatus.Excused => "بعذر",
                _ => status.ToString(),
            };
    }
}

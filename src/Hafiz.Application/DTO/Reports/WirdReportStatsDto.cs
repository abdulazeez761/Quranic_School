using System;
using System.Collections.Generic;
using Hafiz.Models;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// إحصائيات مجمّعة عن الأوراد ضمن نطاق التصفية: أعداد ونِسَب إنجاز وحجم (صفحات).
    /// </summary>
    public class WirdReportStatsDto
    {
        public int TotalAssignments { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int UpcomingCount { get; set; }

        // المكافئ بالصفحات (يُحسب في الذاكرة لأن WirdPageCalculator غير قابل للترجمة إلى SQL).
        public decimal TotalEquivalentPages { get; set; }
        public decimal CompletedEquivalentPages { get; set; }

        public double CompletionRate =>
            TotalAssignments > 0
                ? Math.Round((double)CompletedCount / TotalAssignments * 100, 1)
                : 0;

        /// <summary>تفصيل حسب نوع الورد (حفظ، مراجعة، تجويد...).</summary>
        public List<WirdTypeStat> ByType { get; set; } = new();
    }

    /// <summary>سطر إحصائي لنوع ورد واحد.</summary>
    public class WirdTypeStat
    {
        public AssignmentType Type { get; set; }
        public int Total { get; set; }
        public int Completed { get; set; }
        public decimal Pages { get; set; }

        public double CompletionRate =>
            Total > 0 ? Math.Round((double)Completed / Total * 100, 1) : 0;
    }
}

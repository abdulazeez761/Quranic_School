using System;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// سطر ترتيب أداء الطالب ضمن التقرير: عدد الأوراد ونسبة إنجازها والحجم بالصفحات.
    /// </summary>
    public class StudentRankingRow
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public int TotalWirds { get; set; }
        public int CompletedWirds { get; set; }
        public decimal TotalPages { get; set; }

        public double CompletionRate =>
            TotalWirds > 0 ? Math.Round((double)CompletedWirds / TotalWirds * 100, 1) : 0;
    }
}

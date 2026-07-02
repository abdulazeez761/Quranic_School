using System;
using Hafiz.Models;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// معايير تصفية تقرير الأوراد. تُبنى من معطيات الاستعلام (query string) وتُعاد للعرض
    /// حتى تبقى الفلاتر المختارة ظاهرة بعد إرسال النموذج.
    /// </summary>
    public class WirdReportFilterDto
    {
        // نطاق المركز: يُفرض تلقائياً للمشرف، واختياري لمشرف النظام (null = كل المراكز).
        public Guid? InstituteId { get; set; }

        public Guid? ClassId { get; set; }
        public Guid? StudentId { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public AssignmentType? Type { get; set; }

        /// <summary>"completed" أو "pending" أو null (الكل).</summary>
        public string? Status { get; set; }

        /// <summary>إظهار قسم ترتيب الطلاب (اختياري). افتراضياً مُفعّل.</summary>
        public bool ShowRankings { get; set; } = true;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        /// <summary>true للمكتمل، false للمعلّق، null لعدم التصفية حسب الإنجاز.</summary>
        public bool? IsCompleted =>
            string.Equals(Status, "completed", StringComparison.OrdinalIgnoreCase) ? true
            : string.Equals(Status, "pending", StringComparison.OrdinalIgnoreCase) ? false
            : (bool?)null;
    }
}

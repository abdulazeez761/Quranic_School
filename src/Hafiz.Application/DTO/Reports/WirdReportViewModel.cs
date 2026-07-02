using System;
using System.Collections.Generic;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// خيار بسيط لقائمة منسدلة. نتجنّب استخدام SelectListItem حتى تبقى طبقة التطبيق
    /// مستقلّة عن ASP.NET Core MVC.
    /// </summary>
    public record SelectOption(string Value, string Text);

    /// <summary>
    /// النموذج الكامل لصفحة تقرير الأوراد: الفلاتر، الإحصائيات، ترتيب الطلاب،
    /// التفاصيل المُصفّحة، وخيارات القوائم المنسدلة.
    /// </summary>
    public class WirdReportViewModel
    {
        public WirdReportFilterDto Filter { get; set; } = new();
        public WirdReportStatsDto Stats { get; set; } = new();

        public List<StudentRankingRow> TopStudents { get; set; } = new();
        public List<WirdReportDetailRow> Details { get; set; } = new();

        // خيارات التصفية للعرض
        public List<SelectOption> ClassOptions { get; set; } = new();
        public List<SelectOption> StudentOptions { get; set; } = new();
        public List<SelectOption> InstituteOptions { get; set; } = new();

        public bool IsCrossCenter { get; set; }

        // ترقيم الصفحات (يخصّ جدول التفاصيل فقط)
        public int TotalCount { get; set; }
        public int TotalPages =>
            Filter.PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / Filter.PageSize) : 0;
        public bool HasPreviousPage => Filter.Page > 1;
        public bool HasNextPage => Filter.Page < TotalPages;
    }
}

using System;
using System.Collections.Generic;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// خيار بسيط لقائمة منسدلة. نتجنّب استخدام SelectListItem حتى تبقى طبقة التطبيق
    /// مستقلّة عن ASP.NET Core MVC.
    /// </summary>
    public record SelectOption(string Value, string Text);

    public class MatnReportStatsDto
    {
        public int TotalAssignments { get; set; }
        public int MemorizationCount { get; set; }
        public int RevisionCount { get; set; }
        public int MudarasahCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal TotalVerses { get; set; }
        public decimal CompletedVerses { get; set; }
        public decimal CompletedLines { get; set; }
        public decimal CompletedPages { get; set; }
        public decimal CompletedChapters { get; set; }
        public decimal CompletedHadiths { get; set; }
        public int CompletionRate =>
            TotalAssignments > 0
                ? (int)Math.Round((double)CompletedCount / TotalAssignments * 100)
                : 0;

        public List<(string Label, decimal Value)> GetActiveCompletedUnits()
        {
            var list = new List<(string Label, decimal Value)>();
            if (CompletedVerses > 0) list.Add(("أبيات", CompletedVerses));
            if (CompletedLines > 0) list.Add(("سطور", CompletedLines));
            if (CompletedPages > 0) list.Add(("صفحات", CompletedPages));
            if (CompletedHadiths > 0) list.Add(("أحاديث", CompletedHadiths));
            if (CompletedChapters > 0) list.Add(("أبواب", CompletedChapters));
            return list;
        }
    }

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

        // ── أوراد المتون العلمية ──
        public MatnReportStatsDto MatnStats { get; set; } = new();
        public List<Hafiz.DTOs.Matn.MatnAssignmentDto> MatnDetails { get; set; } = new();
        public string ActiveTab { get; set; } = "quran";

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

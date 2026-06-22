namespace Hafiz.DTOs.Dashboard
{
    /// <summary>
    /// الفترة الزمنية التي تُحسب عليها إحصائيات الأوراد في الداشبورد.
    /// </summary>
    public enum DashboardPeriod
    {
        /// <summary>إجمالي تراكمي لكل الفترات (بدون فلترة بالتاريخ).</summary>
        AllTime,

        /// <summary>الشهر الحالي فقط.</summary>
        Month,

        /// <summary>اليوم فقط.</summary>
        Today,
    }
}

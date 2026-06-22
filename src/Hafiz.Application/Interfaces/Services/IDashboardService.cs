using Hafiz.DTOs.Dashboard;

namespace Hafiz.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// يجلب إحصائيات الداشبورد الإدارية من قاعدة البيانات.
        /// إذا تم تمرير instituteId يتم الفلترة على مركز معين فقط، وإلا يجلب كل المنصة.
        /// أما <paramref name="period"/> فتُحدد الفترة الزمنية التي تُحسب عليها إحصائيات الأوراد.
        /// </summary>
        Task<DashboardStatsDto> GetDashboardStatsAsync(
            Guid? instituteId = null,
            DashboardPeriod period = DashboardPeriod.AllTime
        );
    }
}

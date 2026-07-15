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
            DashboardPeriod period = DashboardPeriod.AllTime,
            DateTime? today = null
        );

        /// <summary>
        /// يجلب صفحة إضافية من نشاطات اليوم (أوراد أو حضور) لدعم زر "تحميل المزيد".
        /// </summary>
        Task<DashboardActivityPage> GetActivityPageAsync(
            Guid? instituteId,
            DashboardActivityCategory category,
            int page,
            int pageSize
        );
    }
}

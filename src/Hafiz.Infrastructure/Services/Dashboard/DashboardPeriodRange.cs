using System;
using Hafiz.DTOs.Dashboard;

namespace Hafiz.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// يُرجع نطاق التاريخ [from, toExclusive) المقابل للفترة المطلوبة على لوحة الإدارة.
    /// <c>from == null</c> تعني "بدون فلترة" (إجمالي تراكمي).
    /// </summary>
    internal static class DashboardPeriodRange
    {
        internal static (DateTime? from, DateTime? toExclusive) Resolve(DashboardPeriod period)
        {
            var today = DateTime.Today;
            return period switch
            {
                DashboardPeriod.Today => (today, today.AddDays(1)),
                DashboardPeriod.Month => (
                    new DateTime(today.Year, today.Month, 1),
                    new DateTime(today.Year, today.Month, 1).AddMonths(1)
                ),
                _ => (null, null),
            };
        }
    }
}

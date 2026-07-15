using System;
using Microsoft.AspNetCore.Http;

namespace Hafiz.Web.Helpers
{
    public static class TimeZoneHelper
    {
        private const string TimeZoneCookieName = "UserTimeZone";

        public static TimeZoneInfo GetUserTimeZone(HttpContext context)
        {
            if (
                context != null
                && context.Request.Cookies.TryGetValue(TimeZoneCookieName, out var timeZoneId)
                && !string.IsNullOrEmpty(timeZoneId)
            )
            {
                try
                {
                    var decodedId = Uri.UnescapeDataString(timeZoneId);
                    return TimeZoneInfo.FindSystemTimeZoneById(decodedId);
                }
                catch
                {
                    // Ignore and fallback
                }
            }
            try
            {
                // Fallback to Jordan timezone
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Amman");
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        }

        public static DateTime ToUserTime(this DateTime utcDateTime, HttpContext context)
        {
            var utc =
                utcDateTime.Kind == DateTimeKind.Utc
                    ? utcDateTime
                    : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            var userTimeZone = GetUserTimeZone(context);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, userTimeZone);
        }

        public static DateTime GetUserToday(HttpContext context)
        {
            var userTimeZone = GetUserTimeZone(context);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone).Date;
        }

        public static DateTime GetUserNow(HttpContext context)
        {
            var userTimeZone = GetUserTimeZone(context);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
        }
    }
}

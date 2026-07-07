using System;

namespace Hafiz.Web.Helpers
{
    public static class StudentReportViewHelpers
    {
        public static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length == 0)
                return string.Empty;

            var second = parts.Length > 1 ? parts[1][0].ToString() : string.Empty;
            return $"{parts[0][0]}{second}";
        }

        public static string AttendanceClass(double rate) =>
            rate >= 80 ? "good" : rate >= 50 ? "mid" : "low";
    }
}

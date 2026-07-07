using System;

namespace Hafiz.DTOs.Dashboard
{
    /// <summary>نوع النشاط المعروض في قائمة "النشاط الأخير" على لوحة الإدارة.</summary>
    public enum DashboardActivityKind
    {
        WirdMemorization = 0,
        WirdRevision = 1,
        StudentAttendance = 2,
        TeacherAttendance = 3,
    }

    /// <summary>فئة النشاط المعروض في قسم/تبويب مستقل على لوحة الإدارة.</summary>
    public enum DashboardActivityCategory
    {
        /// <summary>أوراد الحفظ والمراجعة.</summary>
        Wirds = 0,

        /// <summary>حضور الطلاب والمعلمين.</summary>
        Attendance = 1,
    }

    /// <summary>عنصر نشاط واحد يظهر ضمن قائمة النشاطات على لوحة الإدارة.</summary>
    public sealed class DashboardActivityItem
    {
        public DashboardActivityKind Kind { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }
}

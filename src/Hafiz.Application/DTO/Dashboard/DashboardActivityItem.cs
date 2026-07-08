using System;
using Hafiz.Models;

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

    /// <summary>ملخّص أعداد الحضور لصف واحد في يوم واحد — يُلوَّن في الواجهة لجذب الانتباه.</summary>
    public sealed class AttendanceCountsSummary
    {
        public int Present { get; init; }
        public int Absent { get; init; }
        public int Excused { get; init; }
        public int Late { get; init; }
        public int Total { get; init; }
    }

    /// <summary>عنصر نشاط واحد يظهر ضمن قائمة النشاطات على لوحة الإدارة.</summary>
    public sealed class DashboardActivityItem
    {
        public DashboardActivityKind Kind { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }

        /// <summary>يُعبَّأ فقط لعناصر حضور الطلاب — تُعرض أرقامه بألوان دلالية.</summary>
        public AttendanceCountsSummary? Counts { get; init; }

        /// <summary>يُعبَّأ فقط لعناصر حضور المعلم — تُعرض حالته بلون دلالي.</summary>
        public AttendanceStatus? TeacherStatus { get; init; }
    }
}

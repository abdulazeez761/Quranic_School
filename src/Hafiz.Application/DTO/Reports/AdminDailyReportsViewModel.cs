using System;
using System.Collections.Generic;
using System.Linq;
using Hafiz.Models;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// تقرير يومي عن حضور وغياب وأوراد الطلاب مفصّلاً حسب كل شعبة (حلقة).
    /// </summary>
    public class AdminDailyReportsViewModel
    {
        public DateTime Date { get; set; }
        public List<ClassDailyReport> Classes { get; set; } = new();

        // إجماليات على مستوى المركز لهذا اليوم
        public int TotalStudents => Classes.Sum(c => c.TotalStudents);
        public int TotalPresent => Classes.Sum(c => c.PresentCount);
        public int TotalLate => Classes.Sum(c => c.LateCount);
        public int TotalAbsent => Classes.Sum(c => c.AbsentCount);
        public int TotalExcused => Classes.Sum(c => c.ExcusedCount);
        public int TotalPending => Classes.Sum(c => c.PendingCount);
        public int TotalWirds => Classes.Sum(c => c.Students.Sum(s => s.Wirds.Count));
    }

    public class ClassDailyReport
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public List<StudentDailyRow> Students { get; set; } = new();

        public int TotalStudents => Students.Count;
        public int PresentCount => Students.Count(s => s.Status == AttendanceStatus.Present);
        public int LateCount => Students.Count(s => s.Status == AttendanceStatus.Late);
        public int AbsentCount => Students.Count(s => s.Status == AttendanceStatus.Absent);
        public int ExcusedCount => Students.Count(s => s.Status == AttendanceStatus.Excused);

        // الطلاب الذين لم يُسجّل لهم حضور بعد لهذا اليوم
        public int PendingCount =>
            Students.Count(s => s.Status == null || s.Status == AttendanceStatus.Pending);

        public double AttendanceRate =>
            TotalStudents > 0
                ? Math.Round((double)(PresentCount + LateCount) / TotalStudents * 100, 1)
                : 0;
    }

    public class StudentDailyRow
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;

        // null يعني أنه لم يُسجّل حضور لهذا الطالب في هذا اليوم
        public AttendanceStatus? Status { get; set; }

        // الأوراد المسندة لهذا الطالب في هذا اليوم
        public List<WirdAssignment> Wirds { get; set; } = new();
    }
}

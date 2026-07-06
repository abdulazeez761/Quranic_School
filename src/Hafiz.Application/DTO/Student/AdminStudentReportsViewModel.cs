using System;
using System.Collections.Generic;
using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.DTOs.Student
{
    public class AdminStudentReportsViewModel
    {
        public List<StudentReportRow> Students { get; set; } = new();
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public Guid? ClassId { get; set; }
        public string? Search { get; set; }
    }

    public class StudentReportRow
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public decimal TotalMemorizedPages { get; set; }
        public int MemorizedJuz { get; set; }
        public decimal ReviewedPages { get; set; }
        public int TotalWirds { get; set; }
        public int CompletedWirds { get; set; }
        public double AttendanceRate { get; set; }
        public int TotalAttendance { get; set; }
        public bool IsHafiz { get; set; }
        public TajwidLevel TajwidLevel { get; set; }
        public int Age { get; set; }
        public DateTime? LastWirdDate { get; set; }
        public bool IsInactiveWarning { get; set; }
        public int UnreadNotesCount { get; set; }
    }
}

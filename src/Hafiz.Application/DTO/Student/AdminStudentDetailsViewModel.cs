using System;
using System.Collections.Generic;
using Hafiz.DTOs.Wird;
using Hafiz.Models;
using StudentModel = Hafiz.Models.Student;

namespace Hafiz.DTOs.Student
{
    public class AdminStudentDetailsViewModel
    {
        public StudentModel Student { get; set; } = new();
        public PaginatedWirdsResponse PaginatedWirds { get; set; } = new();
        public List<StudentAttendance> Attendance { get; set; } = new();
        public IEnumerable<ParentNote> ParentNotes { get; set; } = new List<ParentNote>();

        public double AttendanceRate { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }

        public string? WirdStatus { get; set; }
        public string? WirdType { get; set; }
        public DateTime? AttendanceFromDate { get; set; }
        public DateTime? AttendanceToDate { get; set; }
        public string ActiveTab { get; set; } = "overview";
    }
}

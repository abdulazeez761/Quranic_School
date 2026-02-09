using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.DTOs.Attendance
{
    public class SaveStudentAttendanceDto
    {
        public Guid StudentID { get; set; }
        public Guid ClassID { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime Date { get; set; }
        public int Hours { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hafiz.DTOs.Attendance
{
    public class PreviousStudentAttendanceDto
    {
        public Guid StudentId { get; set; }
        public int Status { get; set; }
        public Guid ClassId { get; set; }

        public DateTime Date { get; set; }
    }
}

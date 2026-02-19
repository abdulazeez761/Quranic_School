using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hafiz.DTOs.Attendance
{
    public class StudentAttendanceDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public PreviousStudentAttendanceDto? PrevAttendance { get; set; }
    }
}

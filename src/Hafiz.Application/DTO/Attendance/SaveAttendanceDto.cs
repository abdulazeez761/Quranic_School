using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.DTOs
{
    public class SaveTeacherAttendanceDto
    {
        public Guid TeacherId { get; set; }
        public Guid ClassID { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime Date { get; set; }
        public int Hours { get; set; }
    }
}

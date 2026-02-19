using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.DTOs
{
    public class PreviousTeacherAttendanceDto
    {
        public Guid TeacherId { get; set; }
        public int Status { get; set; }
        public Guid ClassId { get; set; }
        public double WorkingHours { get; set; }
        public DateTime Date { get; set; }
    }
}

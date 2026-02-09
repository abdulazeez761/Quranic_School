using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hifz.DTOs
{
    public class TeacherAttendanceDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public PreviousTeacherAttendanceDto? PrevAttendance { get; set; }
    }
}

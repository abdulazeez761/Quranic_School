using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hafiz.Models
{
    public class TeacherAttendance
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public Teacher Teacher { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [ForeignKey(nameof(ClassId))]
        public Class Class { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public int WorkingHours { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.Application.DTO.Wird
{
    public class AssignWirdsBatchDto
    {
        public Guid StudentId { get; set; }
        public Guid? ClassId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public List<WirdAssignment> Wirds { get; set; } = new();
    }
}

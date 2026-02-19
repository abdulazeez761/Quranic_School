using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hafiz.DTOs.Wird
{
    public class UpdateNoteRequest
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
    }
}

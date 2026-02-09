using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hifz.DTOs.Wird
{
    public class UpdateNoteRequest
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
    }
}

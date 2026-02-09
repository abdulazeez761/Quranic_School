using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.DTOs.Wird
{
    public class UpdateStatusRequest
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Models;

namespace Hafiz.DTOs.Wird
{
    public class UpdateStatusRequest
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
    }
}

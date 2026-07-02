using System;
using Hafiz.Models;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// سطر تفصيلي لورد واحد في جدول التقرير.
    /// </summary>
    public class WirdReportDetailRow
    {
        public Guid WirdId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public AssignmentType Type { get; set; }
        public decimal? Amount { get; set; }
        public WirdUnit? AmountUnit { get; set; }
        public decimal EquivalentPages { get; set; }

        /// <summary>وصف جاهز للعرض (النطاق أو الكمية والوحدة).</summary>
        public string Description { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsUpcoming { get; set; }
        public AssignmentStatus Status { get; set; }
        public string? Note { get; set; }
    }
}

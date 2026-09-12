using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Models;

namespace Hafiz.DTOs.StudentPlan
{
    public class UpsertStudentRoutinePlanDto
    {
        [Required(ErrorMessage = "معرف الطالب مطلوب")]
        public Guid StudentId { get; set; }

        public Guid? ClassId { get; set; }

        public bool MemActive { get; set; } = true;

        [Range(0.1, 604, ErrorMessage = "مقدار الحفظ يجب أن يكون أكبر من 0")]
        public decimal MemAmount { get; set; } = 1.0m;

        public WirdUnit MemUnit { get; set; } = WirdUnit.Pages;

        public decimal? MemEquivalentPages { get; set; }

        public bool RecentRevActive { get; set; } = true;

        [Range(0.1, 604, ErrorMessage = "مقدار المراجعة الصغرى يجب أن يكون أكبر من 0")]
        public decimal RecentRevAmount { get; set; } = 5.0m;

        public WirdUnit RecentRevUnit { get; set; } = WirdUnit.Pages;

        public bool OldRevActive { get; set; } = true;

        [Range(0.1, 30, ErrorMessage = "مقدار المراجعة الكبرى يجب أن يكون أكبر من 0")]
        public decimal OldRevAmount { get; set; } = 1.0m;

        public WirdUnit OldRevUnit { get; set; } = WirdUnit.Juz;

        public bool RecitationActive { get; set; } = true;

        [Range(0.1, 604, ErrorMessage = "مقدار التلاوة يجب أن يكون أكبر من 0")]
        public decimal RecitationAmount { get; set; } = 2.0m;

        public WirdUnit RecitationUnit { get; set; } = WirdUnit.Pages;

        [MaxLength(500, ErrorMessage = "لا يمكن للملاحظة أن تتجاوز 500 حرف")]
        public string? DefaultNote { get; set; }
    }
}

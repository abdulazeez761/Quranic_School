using System;
using System.Collections.Generic;
using Hafiz.Models;

namespace Hafiz.DTOs.Reports
{
    /// <summary>
    /// سطر مبسّط لكل ورد يُستخدم لحساب ترتيب الطلاب. يحمل فقط الحقول اللازمة للتجميع،
    /// فيُجلب من قاعدة البيانات كإسقاط خفيف بدل تحميل الكيانات الكاملة.
    /// </summary>
    public class WirdRankingSourceRow
    {
        public Guid StudentId { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public List<string> ClassNames { get; set; } = new();

        public bool IsCompleted { get; set; }

        public decimal? Amount { get; set; }
        public WirdUnit? AmountUnit { get; set; }
        public decimal? EquivalentPages { get; set; }
    }
}

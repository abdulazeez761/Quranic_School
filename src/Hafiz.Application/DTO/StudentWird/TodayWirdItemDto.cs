using System;

namespace Hafiz.Application.DTO.StudentWird
{
    public class TodayWirdItemDto
    {
        public Guid Id { get; set; }
        public decimal? Amount { get; set; }
        public int? AmountUnit { get; set; }
        public decimal? EquivalentPages { get; set; }
        public int? FromSurah { get; set; }
        public string? FromAyah { get; set; }
        public int? ToSurah { get; set; }
        public int? ToAyah { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public bool IsUpcoming { get; set; }
        public bool IsCompleted { get; set; }
    }
}

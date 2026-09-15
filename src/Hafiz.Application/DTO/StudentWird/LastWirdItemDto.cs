namespace Hafiz.Application.DTO.StudentWird
{
    public class LastWirdItemDto
    {
        public int? FromSurah { get; set; }
        public string? FromAyah { get; set; }
        public int? ToSurah { get; set; }
        public int? ToAyah { get; set; }
        public decimal? Amount { get; set; }
        public int? AmountUnit { get; set; }
        public string? AssignedDate { get; set; }
    }
}

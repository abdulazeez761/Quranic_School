namespace Hafiz.Application.DTO.StudentWird
{
    public class TodayWirdsDto
    {
        public TodayWirdItemDto? Memorization { get; set; }
        public TodayWirdItemDto? RecentRevision { get; set; }
        public TodayWirdItemDto? OldRevision { get; set; }
        public TodayWirdItemDto? Recitation { get; set; }
    }
}

namespace Hafiz.Domain.Enums;

public enum MemorizationStatus
{
    NotStarted = 0,
    InProgress = 1,
    Memorized = 2
}

public static class MemorizationStatusExtensions
{
    public static string ToArabic(this MemorizationStatus status) => status switch
    {
        MemorizationStatus.NotStarted => "لم يبدأ",
        MemorizationStatus.InProgress => "قيد الحفظ",
        MemorizationStatus.Memorized => "تم الحفظ",
        _ => "غير محدد"
    };
}

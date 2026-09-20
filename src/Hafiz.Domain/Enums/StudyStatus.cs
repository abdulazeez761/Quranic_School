namespace Hafiz.Domain.Enums;

public enum StudyStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}

public static class StudyStatusExtensions
{
    public static string ToArabic(this StudyStatus status) => status switch
    {
        StudyStatus.NotStarted => "لم يبدأ",
        StudyStatus.InProgress => "قيد المدارسة",
        StudyStatus.Completed => "مكتمل",
        _ => "غير محدد"
    };
}

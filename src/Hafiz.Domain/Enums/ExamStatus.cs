namespace Hafiz.Domain.Enums;

public enum ExamStatus
{
    NotTested = 0,
    Passed = 1,
    Failed = 2
}

public static class ExamStatusExtensions
{
    public static string ToArabic(this ExamStatus status) => status switch
    {
        ExamStatus.NotTested => "لم يختبر",
        ExamStatus.Passed => "ناجح",
        ExamStatus.Failed => "راسب",
        _ => "غير محدد"
    };
}

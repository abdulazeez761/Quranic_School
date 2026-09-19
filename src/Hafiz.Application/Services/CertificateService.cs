using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Common.Helper;
using Hafiz.Domain.Enums;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;

namespace Hafiz.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly IStudentMatnProgressRepository _progressRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IInstituteRepository _instituteRepository;

    public CertificateService(
        IStudentMatnProgressRepository progressRepository,
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IInstituteRepository instituteRepository)
    {
        _progressRepository = progressRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _instituteRepository = instituteRepository;
    }

    public async Task<CertificateModel?> GetMatnCertificateAsync(Guid studentMatnProgressId)
    {
        var progress = await _progressRepository.GetByIdAsync(studentMatnProgressId);
        if (progress == null)
            return null;

        // Ensure student has achieved completion in study, memorization, or both
        var isStudyCompleted = progress.StudyStatus == StudyStatus.Completed;
        var isMemCompleted = progress.MemorizationStatus == MemorizationStatus.Memorized;
        if (!isStudyCompleted && !isMemCompleted && progress.ExamStatus != ExamStatus.Passed)
            return null;

        var student = progress.Student;
        var studentUser = student?.StudentInfo;
        var isFemale = student?.sex == Sex.female;
        var studentName = studentUser != null
            ? studentUser.FullName
            : progress.StudentId.ToString();

        // Class and Institute information
        string className = string.Empty;
        string instituteName = "مركز تحفيظ القرآن الكريم والعلوم الشرعية";
        string teacherName = progress.LastUpdatedByTeacher?.TeacherInfo != null
            ? progress.LastUpdatedByTeacher.TeacherInfo.FullName
            : string.Empty;

        if (student?.ClassId.HasValue == true)
        {
            var cls = await _classRepository.GetById(student.ClassId.Value);
            if (cls != null)
            {
                className = cls.Name;
                if (string.IsNullOrEmpty(teacherName) && cls.Teachers != null && cls.Teachers.Any())
                {
                    var leadTeacher = cls.Teachers.FirstOrDefault()?.TeacherInfo;
                    if (leadTeacher != null)
                        teacherName = leadTeacher.FullName;
                }

                if (cls.InstituteId.HasValue)
                {
                    var inst = await _instituteRepository.GetByIdAsync(cls.InstituteId.Value);
                    if (inst != null && !string.IsNullOrEmpty(inst.Name))
                        instituteName = inst.Name;
                }
            }
        }

        // Achievement phrasing
        string certTitle;
        string achievementDesc;
        string scopeDetails;

        if (isStudyCompleted && isMemCompleted)
        {
            certTitle = "شهادة إتمام حفظ ومدارسة متن علمي";
            achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه وفضله حفظ ومدارسة وإتقان متن:";
            scopeDetails = "تم إكمال متطلبات الحفظ المتقن والمدارسة العلمية الكاملة للمتن";
        }
        else if (isMemCompleted)
        {
            certTitle = "شهادة إتمام حفظ وإتقان متن علمي";
            achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه وفضله استظهار وحفظ وإتقان متن:";
            scopeDetails = "تم إكمال حفظ واستظهار أبيات / فصول المتن كاملاً غيباً عن ظهر قلب";
        }
        else
        {
            certTitle = "شهادة إتمام دراسة وضبط متن علمي";
            achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه وفضله مدارسة وفهم وضبط متن:";
            scopeDetails = "تم إكمال مدارسة وشرح وضبط أبواب وفصول المتن بنجاح";
        }

        // Evaluation & Rating
        string? rating = null;
        string? examResultText = null;
        if (progress.Score.HasValue || progress.ExamStatus == ExamStatus.Passed)
        {
            var s = progress.Score ?? 100m;
            if (s >= 90) rating = "ممتاز (Excellent)";
            else if (s >= 80) rating = "جيد جداً (Very Good)";
            else if (s >= 65) rating = "جيد (Good)";
            else if (s >= 50) rating = "مقبول (Fair)";
            else rating = "ناجح";

            examResultText = progress.Score.HasValue
                ? $"بتقدير: {rating} — بنسبة ({progress.Score.Value:0.##}%)"
                : $"بتقدير: {rating}";
        }

        var completionDate = progress.ExamDate ?? progress.MemorizationCompletedAt ?? progress.StudyCompletedAt ?? DateTime.UtcNow;

        return new CertificateModel
        {
            CertificateNumber = $"MATN-{progress.Id.ToString()[..8].ToUpper()}",
            Type = CertificateType.Matn,
            TemplateName = "Matn",
            StudentId = progress.StudentId,
            StudentName = studentName,
            StudentGender = isFemale ? "Female" : "Male",
            CertificateTitle = certTitle,
            SubjectName = progress.Matn?.Title ?? "المتن العلمي",
            SubtitleOrAuthor = !string.IsNullOrEmpty(progress.Matn?.Author) ? $"للإمام / {progress.Matn.Author}" : null,
            AchievementDescription = achievementDesc,
            ScopeDetails = scopeDetails,
            Score = progress.Score,
            Rating = rating,
            ExamResultText = examResultText,
            IssueDate = completionDate,
            IssueDateFormatted = FormatArabicDate(completionDate),
            InstituteName = instituteName,
            ClassName = className,
            TeacherName = !string.IsNullOrEmpty(teacherName) ? teacherName : "معلم الحلقة",
            DirectorName = "إدارة الشؤون التعليمية"
        };
    }

    public async Task<CertificateModel?> GetQuranCertificateAsync(Guid studentId, int? fromJuz = null, int? toJuz = null)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
            return null;

        var studentUser = student.StudentInfo;
        var isFemale = student.sex == Sex.female;
        var studentName = studentUser != null
            ? studentUser.FullName
            : studentId.ToString();

        string className = string.Empty;
        string instituteName = "مركز تحفيظ القرآن الكريم والعلوم الشرعية";
        string teacherName = string.Empty;

        if (student.ClassId.HasValue)
        {
            var cls = await _classRepository.GetById(student.ClassId.Value);
            if (cls != null)
            {
                className = cls.Name;
                if (cls.Teachers != null && cls.Teachers.Any())
                {
                    var leadTeacher = cls.Teachers.FirstOrDefault()?.TeacherInfo;
                    if (leadTeacher != null)
                        teacherName = leadTeacher.FullName;
                }

                if (cls.InstituteId.HasValue)
                {
                    var inst = await _instituteRepository.GetByIdAsync(cls.InstituteId.Value);
                    if (inst != null && !string.IsNullOrEmpty(inst.Name))
                        instituteName = inst.Name;
                }
            }
        }

        string certTitle;
        string subjectName;
        string? authorOrSubtext = "برواية حفص عن عاصم من طريق الشاطبية";
        string achievementDesc;
        string scopeDetails;

        var isHafiz = WirdPageCalculator.IsHafiz(student) || (fromJuz == 1 && toJuz == 30);

        if (isHafiz)
        {
            certTitle = "شهادة ختم القرآن الكريم كاملاً";
            subjectName = "كتاب الله تعالى كاملاً (30 جزءاً)";
            achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة المباركة" : "الطالب المبارك")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه وفضله ختم القرآن الكريم كاملاً غيباً عن ظهر قلب.";
            scopeDetails = "من سورة الفاتحة إلى سورة الناس بحفظ متقن ومراعاة لأحكام التلاوة والتجويد";
        }
        else if (fromJuz.HasValue && toJuz.HasValue)
        {
            if (fromJuz == toJuz)
            {
                certTitle = "شهادة إتمام حفظ جزء من القرآن الكريم";
                subjectName = $"الجزء {fromJuz.Value} من القرآن الكريم";
                achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه حفظ وإتقان الجزء المقرّر غيباً عن ظهر قلب.";
                scopeDetails = $"إتمام الجزء رقم {fromJuz.Value} متقناً مجوداً";
            }
            else
            {
                var count = Math.Abs(toJuz.Value - fromJuz.Value) + 1;
                certTitle = "شهادة إتمام أجزاء من القرآن الكريم";
                subjectName = $"حفظ ({count}) أجزاء من كتاب الله تعالى";
                achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه حفظ وإتقان الأجزاء المقررة غيباً عن ظهر قلب.";
                scopeDetails = $"من الجزء {Math.Min(fromJuz.Value, toJuz.Value)} إلى الجزء {Math.Max(fromJuz.Value, toJuz.Value)}";
            }
        }
        else
        {
            var juzCount = student.MemorizedJuz > 0 ? student.MemorizedJuz : 1;
            certTitle = "شهادة إنجاز في حفظ القرآن الكريم";
            subjectName = $"إتمام حفظ {juzCount} أجزاء من القرآن الكريم";
            achievementDesc = $"تشهد إدارة المركز بأن {(isFemale ? "الطالبة" : "الطالب")} قد {(isFemale ? "أتمّت" : "أتمّ")} بحمد الله وتوفيقه حفظ وإتقان المقرر من كتاب الله العزيز.";
            scopeDetails = $"بواقع {juzCount} أجزاء محفوفة بالإتقان وحسن الأداء";
        }

        var today = DateTime.UtcNow;

        return new CertificateModel
        {
            CertificateNumber = $"QRN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Type = CertificateType.Quran,
            TemplateName = "Quran",
            StudentId = studentId,
            StudentName = studentName,
            StudentGender = isFemale ? "Female" : "Male",
            CertificateTitle = certTitle,
            SubjectName = subjectName,
            SubtitleOrAuthor = authorOrSubtext,
            AchievementDescription = achievementDesc,
            ScopeDetails = scopeDetails,
            Rating = "ممتاز",
            ExamResultText = "بتقدير: ممتاز ومبارك",
            IssueDate = today,
            IssueDateFormatted = FormatArabicDate(today),
            InstituteName = instituteName,
            ClassName = className,
            TeacherName = !string.IsNullOrEmpty(teacherName) ? teacherName : "معلم الحلقة",
            DirectorName = "إدارة الشؤون التعليمية"
        };
    }

    private static string FormatArabicDate(DateTime dt)
    {
        try
        {
            var umalqura = new UmAlQuraCalendar();
            var hYear = umalqura.GetYear(dt);
            var hMonth = umalqura.GetMonth(dt);
            var hDay = umalqura.GetDayOfMonth(dt);
            var gDate = dt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            return $"{hYear:0000}/{hMonth:00}/{hDay:00} هـ الموافق {gDate} م";
        }
        catch
        {
            return dt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + " م";
        }
    }
}

using System;
using System.Collections.Generic;
using Hafiz.Application.DTO.Certificate.TemplateConfiguration;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateModel
{
    public Guid? CertificateId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public CertificateType Type { get; set; } = CertificateType.Matn;
    public string TemplateName { get; set; } = "Default";

    // Student Information
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentGender { get; set; } = "Male"; // "Male" or "Female"
    public string GenderedStudent => StudentGender == "Female" ? "الطالبة" : "الطالب";
    public string GenderedCompleted => StudentGender == "Female" ? "أتمّت" : "أتمّ";

    // Subject / Achievement
    public string CertificateTitle { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? SubtitleOrAuthor { get; set; }
    public string AchievementDescription { get; set; } = string.Empty;
    public string? ScopeDetails { get; set; }

    // Structured memorization scope, so templates can lay the details out instead of parsing prose.
    public int? FromJuz { get; set; }
    public int? ToJuz { get; set; }
    public int? JuzCount { get; set; }
    public int? CompletionPercentage { get; set; }
    public string? Riwayah { get; set; }

    // Evaluation / Rating
    public decimal? Score { get; set; }
    public string? Rating { get; set; }
    public string? ExamResultText { get; set; }

    // Dates (Stored in User's Local Time)
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string IssueDateFormatted { get; set; } = string.Empty;
    public string? HijriDateFormatted { get; set; }
    public string? GregorianDateFormatted { get; set; }

    // Organization & Signatures
    public Guid InstituteId { get; set; }
    public string InstituteName { get; set; } = "مركز تحفيظ القرآن الكريم والعلوم الشرعية";
    public string? ClassName { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string DirectorName { get; set; } = "إدارة الشؤون التعليمية";
    public string? InstituteLogoUrl { get; set; }

    // Verification & QR
    public string VerificationToken { get; set; } = string.Empty;
    public string VerificationUrl { get; set; } = string.Empty;
    public string? QrCodeDataUrl { get; set; } // Base64 PNG data:image/png;base64,...
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;

    // Active Template Configuration
    public TemplateConfiguration.TemplateConfiguration Configuration { get; set; } = new();

    // Metadata dictionary for custom template placeholders
    public Dictionary<string, string> ExtraData { get; set; } = new();
}

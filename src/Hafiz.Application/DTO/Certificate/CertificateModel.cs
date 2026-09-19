using System;
using System.Collections.Generic;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateModel
{
    public string CertificateNumber { get; set; } = string.Empty;
    public CertificateType Type { get; set; } = CertificateType.Matn;
    public string TemplateName { get; set; } = "Matn";

    // Student Information
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentGender { get; set; } = "Male"; // "Male" or "Female"

    // Subject / Achievement
    public string CertificateTitle { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? SubtitleOrAuthor { get; set; }
    public string AchievementDescription { get; set; } = string.Empty;
    public string? ScopeDetails { get; set; }

    // Evaluation / Rating
    public decimal? Score { get; set; }
    public string? Rating { get; set; }
    public string? ExamResultText { get; set; }

    // Dates
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string IssueDateFormatted { get; set; } = string.Empty;

    // Organization & Signatures
    public string InstituteName { get; set; } = "مركز تحفيظ القرآن الكريم والعلوم الشرعية";
    public string? ClassName { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string DirectorName { get; set; } = "إدارة الشؤون التعليمية";
    public string? InstituteLogoUrl { get; set; }

    // Metadata dictionary for custom template placeholders
    public Dictionary<string, string> ExtraData { get; set; } = new();
}
